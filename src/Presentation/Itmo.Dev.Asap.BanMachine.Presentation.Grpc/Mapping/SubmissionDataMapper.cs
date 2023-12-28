using Itmo.Dev.Asap.BanMachine.Application.Models.Submissions;

namespace Itmo.Dev.Asap.BanMachine.Presentation.Grpc.Mapping;

internal static class SubmissionDataMapper
{
    public static SubmissionData MapToModel(this Models.SubmissionData data)
    {
        return new SubmissionData(
            data.SubmissionId.MapToGuid(),
            data.UserId.MapToGuid(),
            data.AssignmentId.MapToGuid(),
            data.FileLink);
    }
}