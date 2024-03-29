namespace Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;

public record SubmissionPairAnalysisResult(
    SubmissionPairAnalysisResultData Data,
    IReadOnlyList<SimilarCodeBlocks> SimilarCodeBlocks);