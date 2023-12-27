using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;

namespace Itmo.Dev.Asap.BanMachine.Application.Contracts.AnalysisResults;

public static class GetResultsData
{
    public sealed record Request(AnalysisId AnalysisId, PageToken? PageToken);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success(
            IReadOnlyCollection<SubmissionPairAnalysisResultData> Data,
            PageToken? PageToken) : Response;

        public sealed record AnalysisNotFound : Response;
    }

    public sealed record PageToken(Guid FirstSubmissionId, Guid SecondSubmissionId);
}