using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;

namespace Itmo.Dev.Asap.BanMachine.Application.Contracts.Analysis;

public static class StartAnalysis
{
    public sealed record Request(AnalysisId AnalysisId);

    public abstract record Response
    {
        private Response() { }

        public sealed record Success : Response;

        public sealed record AlreadyRunning : Response;

        public sealed record AlreadyFinished : Response;
    }
}