namespace Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;

public record SubmissionPairAnalysisResultData(
    Guid FirstSubmissionId,
    Guid SecondSubmissionId,
    double SimilarityScore);