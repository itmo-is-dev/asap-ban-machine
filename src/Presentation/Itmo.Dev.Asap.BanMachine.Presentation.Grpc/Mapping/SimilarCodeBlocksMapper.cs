using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;

namespace Itmo.Dev.Asap.BanMachine.Presentation.Grpc.Mapping;

public static class SimilarCodeBlocksMapper
{
    public static Models.SimilarCodeBlocks MapToGrpcModel(this SimilarCodeBlocks value)
    {
        return new Models.SimilarCodeBlocks
        {
            First = { value.First.Select(MapToGrpcModel) },
            Second = { value.Second.Select(MapToGrpcModel) },
            SimilarityScore = value.SimilarityScore,
        };
    }

    private static Models.CodeBlock MapToGrpcModel(this CodeBlock block)
    {
        return new Models.CodeBlock
        {
            FilePath = block.FilePath,
            LineFrom = block.LineFrom,
            LineTo = block.LineTo,
            Content = block.Content,
        };
    }
}