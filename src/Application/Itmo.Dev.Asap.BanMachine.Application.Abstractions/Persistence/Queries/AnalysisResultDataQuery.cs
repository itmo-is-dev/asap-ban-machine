using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;
using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Asap.BanMachine.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public partial record AnalysisResultDataQuery(
    AnalysisId AnalysisId,
    Guid? FirstSubmissionId,
    Guid? SecondSubmissionId,
    int PageSize);