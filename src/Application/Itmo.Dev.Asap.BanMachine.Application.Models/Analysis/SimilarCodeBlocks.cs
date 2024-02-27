namespace Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;

public record struct SimilarCodeBlocks(CodeBlock[] First, CodeBlock[] Second, double SimilarityScore);