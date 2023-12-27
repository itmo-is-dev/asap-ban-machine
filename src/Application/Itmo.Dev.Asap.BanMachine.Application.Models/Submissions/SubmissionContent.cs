namespace Itmo.Dev.Asap.BanMachine.Application.Models.Submissions;

public record struct SubmissionContent(Guid SubmissionId, IReadOnlyCollection<FileData> Files);