using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;
using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.BanMachine.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public partial record AnalysisResultCodeBlocksQuery(
    AnalysisId AnalysisId,
    Guid FirstSubmissionId,
    Guid SecondSubmissionId,
    double MinimumSimilarityScore,
    int Cursor,
    int PageSize);