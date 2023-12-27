using Itmo.Dev.Asap.BanMachine.Application.Models.Submissions;

namespace Itmo.Dev.Asap.BanMachine.Application.Abstractions.Submissions;

public interface ISubmissionContentLoader
{
    Task<SubmissionContent> LoadAsync(SubmissionData data, CancellationToken cancellationToken);
}