using Itmo.Dev.Asap.BanMachine.Application.Analysis;
using Itmo.Dev.Platform.BackgroundTasks.Configuration.Builders;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Asap.BanMachine.Application.Extensions;

public static class BackgroundTaskConfigurationExtensions
{
    public static IBackgroundTaskConfigurationBuilder AddApplicationBackgroundTasks(
        this IBackgroundTaskConfigurationBuilder builder)
    {
        return builder
            .AddBackgroundTask(x => x
                .WithMetadata<AnalysisTaskMetadata>()
                .WithExecutionMetadata<AnalysisTaskExecutionMetadata>()
                .WithResult<EmptyExecutionResult>()
                .WithError<EmptyError>()
                .HandleBy<AnalysisTask>());
    }
}