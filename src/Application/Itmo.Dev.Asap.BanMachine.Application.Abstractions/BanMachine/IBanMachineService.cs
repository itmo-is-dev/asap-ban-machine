using Itmo.Dev.Asap.BanMachine.Application.Abstractions.BanMachine.Models;
using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;

namespace Itmo.Dev.Asap.BanMachine.Application.Abstractions.BanMachine;

public interface IBanMachineService
{
    IAsyncEnumerable<SubmissionPairAnalysisResult> AnalyseAsync(
        IAsyncEnumerable<BanMachineAnalysisRequest> requests,
        CancellationToken cancellationToken);
}