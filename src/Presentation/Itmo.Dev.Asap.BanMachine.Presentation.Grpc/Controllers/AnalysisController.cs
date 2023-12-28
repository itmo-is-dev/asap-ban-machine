using Grpc.Core;
using Itmo.Dev.Asap.BanMachine.Application.Contracts.Analysis;
using Itmo.Dev.Asap.BanMachine.Application.Contracts.Analysis.Operations;
using Itmo.Dev.Asap.BanMachine.Presentation.Grpc.Mapping;

namespace Itmo.Dev.Asap.BanMachine.Presentation.Grpc.Controllers;

public class AnalysisController : AnalysisService.AnalysisServiceBase
{
    private readonly IAnalysisService _analysisService;

    public AnalysisController(IAnalysisService analysisService)
    {
        _analysisService = analysisService;
    }

    public override async Task<CreateAnalysisResponse> Create(
        CreateAnalysisRequest request,
        ServerCallContext context)
    {
        CreateAnalysis.Response response = await _analysisService.CreateAnalysisAsync(context.CancellationToken);
        return new CreateAnalysisResponse { AnalysisId = response.AnalysisId.ToString() };
    }

    public override async Task<AddAnalysisDataResponse> AddData(
        AddAnalysisDataRequest request,
        ServerCallContext context)
    {
        var applicationRequest = new AddAnalysisData.Request(
            request.AnalysisId.MapToAnalysisId(),
            request.SubmissionData.Select(x => x.MapToModel()).ToArray());

        AddAnalysisData.Response applicationResponse = await _analysisService
            .AddAnalysisDataAsync(applicationRequest, context.CancellationToken);

        return applicationResponse switch
        {
            AddAnalysisData.Response.Success => new AddAnalysisDataResponse { Success = new() },
            AddAnalysisData.Response.AlreadyRunning => new AddAnalysisDataResponse { AlreadyRunning = new() },
            _ => throw new RpcException(new Status(StatusCode.Internal, "Operation yielded unexpected result")),
        };
    }

    public override async Task<StartAnalysisResponse> Start(StartAnalysisRequest request, ServerCallContext context)
    {
        var applicationRequest = new StartAnalysis.Request(request.AnalysisId.MapToAnalysisId());

        StartAnalysis.Response applicationResponse = await _analysisService
            .StartAnalysisAsync(applicationRequest, context.CancellationToken);

        return applicationResponse switch
        {
            StartAnalysis.Response.Success => new StartAnalysisResponse { Success = new() },
            StartAnalysis.Response.AlreadyRunning => new StartAnalysisResponse { AlreadyRunning = new() },
            StartAnalysis.Response.AlreadyFinished => new StartAnalysisResponse { AlreadyFinished = new() },
            _ => throw new RpcException(new Status(StatusCode.Internal, "Operation yielded unexpected result")),
        };
    }
}