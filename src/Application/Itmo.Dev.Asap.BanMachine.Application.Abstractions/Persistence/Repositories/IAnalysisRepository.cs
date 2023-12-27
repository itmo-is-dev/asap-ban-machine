using Itmo.Dev.Asap.BanMachine.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;
using Itmo.Dev.Asap.BanMachine.Application.Models.Submissions;

namespace Itmo.Dev.Asap.BanMachine.Application.Abstractions.Persistence.Repositories;

public interface IAnalysisRepository
{
    Task<AnalysisId> CreateAsync(CancellationToken cancellationToken);

    IAsyncEnumerable<SubmissionDataPair> QueryDataPairsAsync(
        AnalysisDataQuery query,
        CancellationToken cancellationToken);

    IAsyncEnumerable<SubmissionPairAnalysisResultData> QueryAnalysisResultDataAsync(
        AnalysisResultDataQuery query,
        CancellationToken cancellationToken);

    IAsyncEnumerable<SimilarCodeBlocks> QueryAnalysisResultCodeBlocks(
        AnalysisResultCodeBlocksQuery query,
        CancellationToken cancellationToken);

    Task AddDataAsync(
        AnalysisId analysisId,
        IReadOnlyCollection<SubmissionData> data,
        CancellationToken cancellationToken);

    Task AddAnalysisResultAsync(
        AnalysisId analysisId,
        SubmissionPairAnalysisResult result,
        CancellationToken cancellationToken);
}