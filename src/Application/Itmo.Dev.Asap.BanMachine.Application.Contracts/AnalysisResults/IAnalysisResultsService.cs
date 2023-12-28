namespace Itmo.Dev.Asap.BanMachine.Application.Contracts.AnalysisResults;

public interface IAnalysisResultsService
{
    Task<GetResultsData.Response> GetAnalysisResultsDataAsync(
        GetResultsData.Request request,
        CancellationToken cancellationToken);

    Task<GetResultCodeBlocks.Response> GetAnalysisResultCodeBlocksAsync(
        GetResultCodeBlocks.Request request,
        CancellationToken cancellationToken);
}