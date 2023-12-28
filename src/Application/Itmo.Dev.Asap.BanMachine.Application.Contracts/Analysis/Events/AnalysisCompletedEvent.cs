using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;
using Itmo.Dev.Platform.Events;

namespace Itmo.Dev.Asap.BanMachine.Application.Contracts.Analysis.Events;

public record AnalysisCompletedEvent(AnalysisId AnalysisId) : IEvent;