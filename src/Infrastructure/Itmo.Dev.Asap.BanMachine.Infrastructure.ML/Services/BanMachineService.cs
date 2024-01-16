using CliWrap;
using CliWrap.Buffered;
using Itmo.Dev.Asap.BanMachine.Application.Abstractions.BanMachine;
using Itmo.Dev.Asap.BanMachine.Application.Abstractions.BanMachine.Models;
using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Extensions;

public class BanMachineService : IBanMachineService
{
    private const string PythonScriptPath = null;

    public async Task<string> RunPythonScriptAsync(string dir1, string dir2, string resultDir)
    {
        string arguments = $"\"{dir1}\" \"{dir2}\" \"{resultDir}\"";
        BufferedCommandResult result = await Cli.Wrap("python")
            .WithArguments(arguments)
            .ExecuteBufferedAsync();
        return result.StandardOutput;
    }

    public async IAsyncEnumerable<SubmissionPairAnalysisResult> AnalyseAsync(IAsyncEnumerable<BanMachineAnalysisRequest> requests, CancellationToken cancellationToken)
    {
        await foreach (BanMachineAnalysisRequest request in requests.WithCancellation(cancellationToken))
        {
            
        }
    }
}