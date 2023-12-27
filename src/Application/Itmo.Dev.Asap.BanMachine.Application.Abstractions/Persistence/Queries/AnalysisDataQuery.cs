using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;
using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.BanMachine.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public partial record AnalysisDataQuery(
    AnalysisId AnalysisId,
    Guid? FirstSubmissionIdCursor,
    Guid? SecondSubmissionIdCursor,
    int PageSize);