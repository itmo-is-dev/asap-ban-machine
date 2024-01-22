using CliWrap;
using Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Services;

public class PythonRestoreBackgroundService : BackgroundService
{
    private readonly MachineLearningOptions _options;
    private readonly ILogger<PythonRestoreBackgroundService> _logger;

    public PythonRestoreBackgroundService(
        IOptions<MachineLearningOptions> options,
        ILogger<PythonRestoreBackgroundService> logger)
    {
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogTrace("Starting python restore");

        string pypiToken = _options.PyPiToken;

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        cts.CancelAfter(TimeSpan.FromSeconds(60));

        await Cli.Wrap("pip")
            .WithValidation(CommandResultValidation.None)
            .WithArguments("install -r requirements.txt")
            .WithWorkingDirectory(Directory.GetCurrentDirectory())
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(outputBuilder))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(errorBuilder))
            .ExecuteAsync(cts.Token);

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