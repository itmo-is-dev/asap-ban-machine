using CliWrap;
using Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

        await Cli.Wrap("/bin/bash")
            .WithArguments("./restore.sh")
            .WithValidation(CommandResultValidation.None)
            .WithWorkingDirectory(Directory.GetCurrentDirectory())
            .ExecuteLoggedAsync(_logger, cancellationToken);

        _logger.LogTrace("Finished python restore");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}