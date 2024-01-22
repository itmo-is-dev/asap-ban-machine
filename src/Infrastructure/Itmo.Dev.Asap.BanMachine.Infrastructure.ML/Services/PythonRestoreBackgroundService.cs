using CliWrap;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Services;

public class PythonRestoreBackgroundService : BackgroundService
{
    private readonly ILogger<PythonRestoreBackgroundService> _logger;

    public PythonRestoreBackgroundService(ILogger<PythonRestoreBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogTrace("Starting python restore");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        cts.CancelAfter(TimeSpan.FromSeconds(120));

        Command packageCommand = Cli.Wrap("pip")
            .WithArguments("install /packages/asap-ban-machine-model.whl")
            .WithValidation(CommandResultValidation.None)
            .WithWorkingDirectory(Directory.GetCurrentDirectory());

        Command requirementsCommand = Cli.Wrap("pip")
            .WithArguments("install -r requirements.txt")
            .WithValidation(CommandResultValidation.None)
            .WithWorkingDirectory(Directory.GetCurrentDirectory());

        await ExecuteLoggedAsync(packageCommand, cts.Token);
        await ExecuteLoggedAsync(requirementsCommand, cts.Token);
    }

    private async Task ExecuteLoggedAsync(Command command, CancellationToken cancellationToken)
    {
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        await command
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(outputBuilder))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(errorBuilder))
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