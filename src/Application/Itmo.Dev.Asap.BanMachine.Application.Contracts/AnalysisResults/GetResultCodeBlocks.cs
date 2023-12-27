using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;

namespace Itmo.Dev.Asap.BanMachine.Application.Contracts.AnalysisResults;

public static class GetResultCodeBlocks
{
    public sealed record Request(
        AnalysisId AnalysisId,
        Guid FirstSubmissionId,
        Guid SecondSubmissionId,
        double MinimumSimilarityScore,
        int Cursor);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success(IReadOnlyCollection<SimilarCodeBlocks> CodeBlocks) : Response;

        public sealed record AnalysisNotFound : Response;
    }
}