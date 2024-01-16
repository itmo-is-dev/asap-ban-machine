using Itmo.Dev.Asap.BanMachine.Application.Abstractions.Submissions;
using Itmo.Dev.Asap.BanMachine.Application.Models.Submissions;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.ContentLoader;

public class SubmissionContentLoader : ISubmissionContentLoader
{
    private readonly HttpClient _client;

    public SubmissionContentLoader(HttpClient client)
    {
        _client = client;
    }

    public async Task<SubmissionContent> LoadAsync(SubmissionData data, CancellationToken cancellationToken)
    {
        await using Stream response = await _client.GetStreamAsync(data.FileLink, cancellationToken);
        var ms = new MemoryStream();

        await response.CopyToAsync(ms, cancellationToken);

        return new SubmissionContent(data.SubmissionId, ms);
    }
}