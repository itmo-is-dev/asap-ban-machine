using Itmo.Dev.Asap.BanMachine.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.BanMachine.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.BanMachine.Application.Analysis.AnalysisBackgroundTask;
using Itmo.Dev.Asap.BanMachine.Application.Contracts.AnalysisResults;
using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.BanMachine.Application.AnalysisResults;

public class AnalysisResultsService : IAnalysisResultsService
{
    private readonly IBackgroundTaskRepository _backgroundTaskRepository;
    private readonly IAnalysisRepository _analysisRepository;
    private readonly AnalysisResultsServiceOptions _options;

    public AnalysisResultsService(
        IBackgroundTaskRepository backgroundTaskRepository,
        IAnalysisRepository analysisRepository,
        IOptions<AnalysisResultsServiceOptions> options)
    {
        _backgroundTaskRepository = backgroundTaskRepository;
        _analysisRepository = analysisRepository;
        _options = options.Value;
    }

    public async Task<GetResultsData.Response> GetAnalysisResultsDataAsync(
        GetResultsData.Request request,
        CancellationToken cancellationToken)
    {
        var backgroundTaskQuery = BackgroundTaskQuery.Build(builder => builder
            .WithName(AnalysisTask.Name)
            .WithState(BackgroundTaskState.Completed)
            .WithMetadata(new AnalysisTaskMetadata(request.AnalysisId)));

        BackgroundTask? backgroundTask = await _backgroundTaskRepository
            .QueryAsync(backgroundTaskQuery, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        if (backgroundTask is null)
            return new GetResultsData.Response.AnalysisNotFound();

        var query = AnalysisResultDataQuery.Build(builder => builder
            .WithAnalysisId(request.AnalysisId)
            .WithFirstSubmissionId(request.PageToken?.FirstSubmissionId)
            .WithSecondSubmissionId(request.PageToken?.SecondSubmissionId)
            .WithPageSize(_options.PageSize));

        SubmissionPairAnalysisResultData[] data = await _analysisRepository
            .QueryAnalysisResultDataAsync(query, cancellationToken)
            .ToArrayAsync(cancellationToken);

        GetResultsData.PageToken? pageToken = data.Length.Equals(query.PageSize)
            ? MapToPageToken(data[^1])
            : null;

        return new GetResultsData.Response.Success(data, pageToken);
    }

    public async Task<GetResultCodeBlocks.Response> GetAnalysisResultCodeBlocks(
        GetResultCodeBlocks.Request request,
        CancellationToken cancellationToken)
    {
        var backgroundTaskQuery = BackgroundTaskQuery.Build(builder => builder
            .WithName(AnalysisTask.Name)
            .WithState(BackgroundTaskState.Completed)
            .WithMetadata(new AnalysisTaskMetadata(request.AnalysisId)));

        BackgroundTask? backgroundTask = await _backgroundTaskRepository
            .QueryAsync(backgroundTaskQuery, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        if (backgroundTask is null)
            return new GetResultCodeBlocks.Response.AnalysisNotFound();

        var query = AnalysisResultCodeBlocksQuery.Build(builder => builder
            .WithAnalysisId(request.AnalysisId)
            .WithFirstSubmissionId(request.FirstSubmissionId)
            .WithSecondSubmissionId(request.SecondSubmissionId)
            .WithMinimumSimilarityScore(request.MinimumSimilarityScore)
            .WithCursor(request.Cursor)
            .WithPageSize(_options.PageSize));

        SimilarCodeBlocks[] codeBlocks = await _analysisRepository
            .QueryAnalysisResultCodeBlocks(query, cancellationToken)
            .ToArrayAsync(cancellationToken);

        return new GetResultCodeBlocks.Response.Success(codeBlocks);
    }

    private static GetResultsData.PageToken MapToPageToken(SubmissionPairAnalysisResultData data)
        => new GetResultsData.PageToken(data.FirstSubmissionId, data.SecondSubmissionId);
}