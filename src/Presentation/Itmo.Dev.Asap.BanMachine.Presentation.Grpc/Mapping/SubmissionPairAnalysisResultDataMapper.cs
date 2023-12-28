using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;

namespace Itmo.Dev.Asap.BanMachine.Presentation.Grpc.Mapping;

public static class SubmissionPairAnalysisResultDataMapper
{
    public static Models.SubmissionPairAnalysisResultData MapToGrpcModel(this SubmissionPairAnalysisResultData data)
    {
        return new Models.SubmissionPairAnalysisResultData
        {
            FirstSubmissionId = data.FirstSubmissionId.ToString(),
            SecondSubmissionId = data.SecondSubmissionId.ToString(),
            SimilarityScore = data.SimilarityScore,
        };
    }
}