using CliWrap;
using Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Services;

public class PythonRestoreBackgroundService : BackgroundService
{
    private readonly MachineLearningOptions _options;

    public PythonRestoreBackgroundService(IOptions<MachineLearningOptions> options)
    {
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string pypiToken = _options.PyPiToken;

        await Cli.Wrap("PYTHON RESTORE COMMAND")
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.Null)
            .WithStandardErrorPipe(PipeTarget.Null)
            .ExecuteAsync(stoppingToken);
    }
}