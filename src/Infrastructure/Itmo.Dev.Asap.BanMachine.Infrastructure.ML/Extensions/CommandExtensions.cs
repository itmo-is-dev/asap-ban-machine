using CliWrap;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Extensions;

public static class CommandExtensions
{
    public static async Task<CommandResult> ExecuteLoggedAsync(
        this Command command,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        CommandResult result = await command
            .WithStandardOutputPipe(PipeTarget.Merge(
                PipeTarget.ToStringBuilder(outputBuilder),
                PipeTarget.ToDelegate(Console.WriteLine)))
            .WithStandardErrorPipe(PipeTarget.Merge(
                PipeTarget.ToStringBuilder(errorBuilder),
                PipeTarget.ToDelegate(Console.WriteLine)))
            .ExecuteAsync(cancellationToken);

        string commandText = (outputBuilder, errorBuilder) is ({ Length: 0 }, { Length: 0 })
            ? string.Empty
            : command.ToString();

        if (outputBuilder.Length is not 0)
        {
            logger.LogTrace(
                "Executing command = '{Command}' wrote to stdout: {Message}",
                commandText,
                outputBuilder.ToString());
        }

        if (errorBuilder.Length is not 0)
        {
            logger.LogError(
                "Executing command = '{Command}' wrote to stderr: {Message}",
                commandText,
                errorBuilder.ToString());
        }

        return result;
    }
}