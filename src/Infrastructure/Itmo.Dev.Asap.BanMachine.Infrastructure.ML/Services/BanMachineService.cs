using CliWrap;
using Itmo.Dev.Asap.BanMachine.Application.Abstractions.BanMachine;
using Itmo.Dev.Asap.BanMachine.Application.Abstractions.BanMachine.Models;
using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;
using Itmo.Dev.Asap.BanMachine.Application.Models.Submissions;
using Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Services;

public class BanMachineService : IBanMachineService
{
    private const string WorkingDirectory = ".ban-machine";
    private static readonly string FirstSubmissionPath;
    private static readonly string SecondSubmissionPath;
    private static readonly string ResultsPath;

    private readonly ILogger<BanMachineService> _logger;

    static BanMachineService()
    {
        FirstSubmissionPath = Path.Combine(WorkingDirectory, "first.zip");
        SecondSubmissionPath = Path.Combine(WorkingDirectory, "second.zip");
        ResultsPath = Path.Combine(WorkingDirectory, "results");
    }

    public BanMachineService(ILogger<BanMachineService> logger)
    {
        _logger = logger;
    }

    public async IAsyncEnumerable<SubmissionPairAnalysisResult> AnalyseAsync(
        IAsyncEnumerable<BanMachineAnalysisRequest> requests,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(WorkingDirectory);

        await foreach (BanMachineAnalysisRequest request in requests.WithCancellation(cancellationToken))
        {
            if (File.Exists(FirstSubmissionPath))
                File.Delete(FirstSubmissionPath);

            if (File.Exists(SecondSubmissionPath))
                File.Delete(SecondSubmissionPath);

            await using (SubmissionContent first = request.FirstSubmission)
            {
                await using FileStream file = File.OpenWrite(FirstSubmissionPath);
                await first.Content.CopyToAsync(file, cancellationToken);
            }

            await using (SubmissionContent second = request.SecondSubmission)
            {
                await using FileStream file = File.OpenWrite(SecondSubmissionPath);
                await second.Content.CopyToAsync(file, cancellationToken);
            }

            await Cli.Wrap("python3")
                .WithArguments(builder => builder
                    .Add("main.py")
                    .Add(FirstSubmissionPath)
                    .Add(SecondSubmissionPath)
                    .Add(ResultsPath)
                    .Add(Environment.ProcessorCount))
                .WithWorkingDirectory(Directory.GetCurrentDirectory())
                .WithValidation(CommandResultValidation.None)
                .ExecuteLoggedAsync(_logger, cancellationToken);

            double similarityScore = ParseSimilarityScore();
            SimilarCodeBlocks[] codeBlocks = ParseCodeBlocks();

            var data = new SubmissionPairAnalysisResultData(
                request.FirstSubmission.SubmissionId,
                request.SecondSubmission.SubmissionId,
                similarityScore);

            yield return new SubmissionPairAnalysisResult(data, codeBlocks);
        }
    }

    private double ParseSimilarityScore()
    {
        string scoreString = File.ReadAllText(Path.Combine(ResultsPath, "similarity.txt"));
        return double.Parse(scoreString);
    }

    private SimilarCodeBlocks[] ParseCodeBlocks()
    {
        var serializer = JsonSerializer.Create();

        using FileStream stream = File.OpenRead(Path.Combine(ResultsPath, "suspicious_blocks.json"));
        using var streamReader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(streamReader);

        SimilarCodeBlocks[]? result = serializer.Deserialize<SimilarCodeBlocks[]>(jsonReader);

        if (result is null)
            throw new InvalidOperationException("Failed to parse similar code blocks");

        return result;
    }
}