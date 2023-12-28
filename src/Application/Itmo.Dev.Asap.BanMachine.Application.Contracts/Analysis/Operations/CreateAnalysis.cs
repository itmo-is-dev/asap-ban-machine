using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;

namespace Itmo.Dev.Asap.BanMachine.Application.Contracts.Analysis.Operations;

public static class CreateAnalysis
{
    public sealed record Response(AnalysisId AnalysisId);
}