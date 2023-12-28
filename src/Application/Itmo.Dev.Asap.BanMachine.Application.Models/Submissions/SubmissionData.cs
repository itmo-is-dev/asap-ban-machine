namespace Itmo.Dev.Asap.BanMachine.Application.Models.Submissions;

public record SubmissionData(
    Guid SubmissionId,
    Guid UserId,
    Guid AssignmentId,
    string FileLink);