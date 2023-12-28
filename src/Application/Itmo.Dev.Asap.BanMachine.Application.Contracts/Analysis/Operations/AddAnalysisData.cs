using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;
using Itmo.Dev.Asap.BanMachine.Application.Models.Submissions;

namespace Itmo.Dev.Asap.BanMachine.Application.Contracts.Analysis.Operations;

public static class AddAnalysisData
{
    public sealed record Request(AnalysisId AnalysisId, IReadOnlyCollection<SubmissionData> Data);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success : Response;

        public sealed record AlreadyRunning : Response;
    }
}