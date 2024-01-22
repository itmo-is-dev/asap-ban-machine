using CliWrap;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Services;

public class PythonRestoreBackgroundService : IHostedService
{
    private readonly ILogger<PythonRestoreBackgroundService> _logger;

    public PythonRestoreBackgroundService(ILogger<PythonRestoreBackgroundService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Starting python restore");

        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            cts.CancelAfter(TimeSpan.FromSeconds(60));

            Command command = Cli.Wrap("pip")
                .WithArguments("install /packages/asap-ban-machine-model.whl")
                .WithValidation(CommandResultValidation.None)
                .WithWorkingDirectory(Directory.GetCurrentDirectory());

            await ExecuteLoggedAsync(command, cts.Token);
        }

        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            cts.CancelAfter(TimeSpan.FromSeconds(600));

            Command command = Cli.Wrap("pip")
                .WithArguments("install -r requirements.txt")
                .WithValidation(CommandResultValidation.None)
                .WithWorkingDirectory(Directory.GetCurrentDirectory());

            await ExecuteLoggedAsync(command, cts.Token);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task ExecuteLoggedAsync(Command command, CancellationToken cancellationToken)
    {
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        await command
            .WithStandardOutputPipe(PipeTarget.Merge(
                PipeTarget.ToStringBuilder(outputBuilder),
                PipeTarget.ToDelegate(Console.WriteLine)))
            .WithStandardErrorPipe(PipeTarget.Merge(
                PipeTarget.ToStringBuilder(errorBuilder),
                PipeTarget.ToDelegate(Console.WriteLine)))
            .ExecuteAsync(cancellationToken);

        if (outputBuilder.Length is not 0)
        {
            _logger.LogTrace(
                "ML wrote to stdout: {Message}",
                outputBuilder.ToString());
        }

        if (errorBuilder.Length is not 0)
        {
            _logger.LogTrace(
                "ML wrote to stderr: {Message}",
                errorBuilder.ToString());
        }
    }
}