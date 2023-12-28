using Itmo.Dev.Asap.BanMachine.Application.Contracts.Analysis.Operations;

namespace Itmo.Dev.Asap.BanMachine.Application.Contracts.Analysis;

public interface IAnalysisService
{
    Task<CreateAnalysis.Response> CreateAnalysisAsync(CancellationToken cancellationToken);

    Task<AddAnalysisData.Response> AddAnalysisDataAsync(
        AddAnalysisData.Request request,
        CancellationToken cancellationToken);

    Task<StartAnalysis.Response> StartAnalysisAsync(StartAnalysis.Request request, CancellationToken cancellationToken);
}