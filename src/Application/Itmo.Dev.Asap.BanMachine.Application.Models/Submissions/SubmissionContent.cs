namespace Itmo.Dev.Asap.BanMachine.Application.Models.Submissions;

public readonly record struct SubmissionContent(Guid SubmissionId, Stream Content) : IAsyncDisposable
{
    public ValueTask DisposeAsync()
    {
        return Content.DisposeAsync();
    }
}