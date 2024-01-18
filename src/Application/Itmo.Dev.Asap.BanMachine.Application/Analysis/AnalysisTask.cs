using Itmo.Dev.Asap.BanMachine.Application.Abstractions.BanMachine;
using Itmo.Dev.Asap.BanMachine.Application.Abstractions.BanMachine.Models;
using Itmo.Dev.Asap.BanMachine.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.BanMachine.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.BanMachine.Application.Abstractions.Submissions;
using Itmo.Dev.Asap.BanMachine.Application.Contracts.Analysis.Events;
using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;
using Itmo.Dev.Asap.BanMachine.Application.Models.Submissions;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Postgres.Transactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace Itmo.Dev.Asap.BanMachine.Application.Analysis;

public class AnalysisTask :
    IBackgroundTask<AnalysisTaskMetadata, AnalysisTaskExecutionMetadata, EmptyExecutionResult, EmptyError>
{
    private readonly IBanMachineService _banMachineService;
    private readonly IAnalysisRepository _analysisRepository;
    private readonly AnalysisTaskOptions _options;
    private readonly ISubmissionContentLoader _submissionContentLoader;
    private readonly IPostgresTransactionProvider _transactionProvider;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AnalysisTask> _logger;

    public AnalysisTask(
        IBanMachineService banMachineService,
        IAnalysisRepository analysisRepository,
        IOptions<AnalysisTaskOptions> options,
        ISubmissionContentLoader submissionContentLoader,
        IPostgresTransactionProvider transactionProvider,
        IEventPublisher eventPublisher,
        ILogger<AnalysisTask> logger)
    {
        _banMachineService = banMachineService;
        _analysisRepository = analysisRepository;
        _submissionContentLoader = submissionContentLoader;
        _transactionProvider = transactionProvider;
        _eventPublisher = eventPublisher;
        _logger = logger;
        _options = options.Value;
    }

    public static string Name => nameof(AnalysisTask);

    public async Task<BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>> ExecuteAsync(
        BackgroundTaskExecutionContext<AnalysisTaskMetadata, AnalysisTaskExecutionMetadata> executionContext,
        CancellationToken cancellationToken)
    {
        AnalysisTaskExecutionMetadata executionMetadata = executionContext.ExecutionMetadata;

        do
        {
            var dataQuery = AnalysisDataQuery.Build(builder => builder
                .WithAnalysisId(executionContext.Metadata.AnalysisId)
                .WithFirstSubmissionIdCursor(executionMetadata.FirstSubmissionId)
                .WithSecondSubmissionIdCursor(executionMetadata.SecondSubmissionId)
                .WithPageSize(_options.PageSize));

            SubmissionDataPair[] data = await _analysisRepository
                .QueryDataPairsAsync(dataQuery, cancellationToken)
                .ToArrayAsync(cancellationToken);

            IAsyncEnumerable<BanMachineAnalysisRequest> contents = data
                .ToAsyncEnumerable()
                .SelectAwait(pair => Map(pair, cancellationToken));

            IAsyncEnumerable<SubmissionPairAnalysisResult> results = _banMachineService
                .AnalyseAsync(contents, cancellationToken);

            await using NpgsqlTransaction transaction = await _transactionProvider
                .CreateTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            await foreach (SubmissionPairAnalysisResult result in results)
            {
                _logger.LogTrace(
                    "Saving analysis result, first = {FirstSubmissionId}, second = {SecondSubmissionId}, similarity = {Similarity}, code blocks count = {CodeBlocksCount}",
                    result.Data.FirstSubmissionId,
                    result.Data.SecondSubmissionId,
                    result.Data.SimilarityScore,
                    result.SimilarCodeBlocks.Count);

                await _analysisRepository.AddAnalysisResultAsync(
                    executionContext.Metadata.AnalysisId,
                    result,
                    cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            if (data.Length < _options.PageSize)
                break;

            SubmissionDataPair lastPair = data[^1];

            executionMetadata.FirstSubmissionId = lastPair.First.SubmissionId;
            executionMetadata.SecondSubmissionId = lastPair.Second.SubmissionId;
        }
        while (true);

        var evt = new AnalysisCompletedEvent(executionContext.Metadata.AnalysisId);
        await _eventPublisher.PublishAsync(evt, default);

        return new BackgroundTaskExecutionResult<EmptyExecutionResult, EmptyError>.Success(EmptyExecutionResult.Value);
    }

    private async ValueTask<BanMachineAnalysisRequest> Map(
        SubmissionDataPair dataPair,
        CancellationToken cancellationToken)
    {
        Task<SubmissionContent> fistContentTask = _submissionContentLoader
            .LoadAsync(dataPair.First, cancellationToken);

        Task<SubmissionContent> secondContentTask = _submissionContentLoader
            .LoadAsync(dataPair.Second, cancellationToken);

        await Task.WhenAll(fistContentTask, secondContentTask);

        return new BanMachineAnalysisRequest(
            FirstSubmission: fistContentTask.Result,
            SecondSubmission: secondContentTask.Result);
    }
}