using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;

namespace Itmo.Dev.Asap.BanMachine.Application.Analysis;

public record AnalysisTaskMetadata(AnalysisId AnalysisId) : IBackgroundTaskMetadata;