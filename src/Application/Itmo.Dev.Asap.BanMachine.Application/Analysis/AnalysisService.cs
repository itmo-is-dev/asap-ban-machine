using Itmo.Dev.Asap.BanMachine.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.BanMachine.Application.Contracts.Analysis;
using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;
using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Managing;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.Locking;

namespace Itmo.Dev.Asap.BanMachine.Application.Analysis;

public class AnalysisService : IAnalysisService
{
    private readonly IAnalysisRepository _analysisRepository;
    private readonly IBackgroundTaskRepository _backgroundTaskRepository;
    private readonly ILockingService _lockingService;
    private readonly IBackgroundTaskRunner _backgroundTaskRunner;

    public AnalysisService(
        IAnalysisRepository analysisRepository,
        IBackgroundTaskRepository backgroundTaskRepository,
        ILockingService lockingService,
        IBackgroundTaskRunner backgroundTaskRunner)
    {
        _analysisRepository = analysisRepository;
        _backgroundTaskRepository = backgroundTaskRepository;
        _lockingService = lockingService;
        _backgroundTaskRunner = backgroundTaskRunner;
    }

    public async Task<CreateAnalysis.Response> CreateAnalysisAsync(CancellationToken cancellationToken)
    {
        AnalysisId analysisId = await _analysisRepository.CreateAsync(cancellationToken);
        return new CreateAnalysis.Response(analysisId);
    }

    public async Task<AddAnalysisData.Response> AddAnalysisDataAsync(
        AddAnalysisData.Request request,
        CancellationToken cancellationToken)
    {
        var backgroundTaskQuery = BackgroundTaskQuery.Build(builder => builder
            .WithName(AnalysisTask.Name)
            .WithActiveState()
            .WithMetadata(new AnalysisTaskMetadata(request.AnalysisId)));

        BackgroundTask? backgroundTask = await _backgroundTaskRepository
            .QueryAsync(backgroundTaskQuery, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        if (backgroundTask is not null)
            return new AddAnalysisData.Response.AlreadyRunning();

        await _analysisRepository.AddDataAsync(request.AnalysisId, request.Data, cancellationToken);

        return new AddAnalysisData.Response.Success();
    }

    public async Task<StartAnalysis.Response> StartAnalysisAsync(
        StartAnalysis.Request request,
        CancellationToken cancellationToken)
    {
        var metadata = new AnalysisTaskMetadata(request.AnalysisId);

        await using ILockHandle lockHandle = await _lockingService.AcquireAsync(metadata, cancellationToken);

        var backgroundTaskQuery = BackgroundTaskQuery.Build(builder => builder
            .WithName(AnalysisTask.Name)
            .WithMetadata(metadata));

        BackgroundTask? backgroundTask = await _backgroundTaskRepository
            .QueryAsync(backgroundTaskQuery, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);

        if (backgroundTask is { State: BackgroundTaskState.Completed })
            return new StartAnalysis.Response.AlreadyFinished();

        if (backgroundTask is not null)
            return new StartAnalysis.Response.AlreadyRunning();

        await _backgroundTaskRunner
            .StartBackgroundTask
            .WithMetadata(metadata)
            .WithExecutionMetadata(new AnalysisTaskExecutionMetadata())
            .RunWithAsync<AnalysisTask>(cancellationToken);

        return new StartAnalysis.Response.Success();
    }
}