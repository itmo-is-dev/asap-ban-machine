using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;

namespace Itmo.Dev.Asap.BanMachine.Application.Analysis;

public class AnalysisTaskExecutionMetadata : IBackgroundTaskExecutionMetadata
{
    public Guid? FirstSubmissionId { get; set; }

    public Guid? SecondSubmissionId { get; set; }
}