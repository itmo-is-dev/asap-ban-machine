using Grpc.Core;
using Itmo.Dev.Asap.BanMachine.Application.Contracts.AnalysisResults;
using Itmo.Dev.Asap.BanMachine.Presentation.Grpc.Mapping;
using Newtonsoft.Json;

namespace Itmo.Dev.Asap.BanMachine.Presentation.Grpc.Controllers;

public class AnalysisResultsController : AnalysisResultsService.AnalysisResultsServiceBase
{
    private readonly IAnalysisResultsService _service;

    public AnalysisResultsController(IAnalysisResultsService service)
    {
        _service = service;
    }

    public override async Task<GetAnalysisResultsDataResponse> GetAnalysisResultsData(
        GetAnalysisResultsDataRequest request,
        ServerCallContext context)
    {
        GetResultsData.PageToken? pageToken = request.PageToken is null
            ? null
            : JsonConvert.DeserializeObject<GetResultsData.PageToken>(request.PageToken);

        var applicationRequest = new GetResultsData.Request(request.AnalysisId.MapToAnalysisId(), pageToken);

        GetResultsData.Response applicationResponse = await _service
            .GetAnalysisResultsDataAsync(applicationRequest, context.CancellationToken);

        return applicationResponse switch
        {
            GetResultsData.Response.Success success => new GetAnalysisResultsDataResponse
            {
                Success = new GetAnalysisResultsDataResponse.Types.Success
                {
                    Data = { success.Data.Select(x => x.MapToGrpcModel()) },
                    PageToken = success.PageToken is null ? null : JsonConvert.SerializeObject(success.PageToken),
                },
            },

            GetResultsData.Response.AnalysisNotFound => new GetAnalysisResultsDataResponse { AnalysisNotFound = new() },

            _ => throw new RpcException(new Status(StatusCode.Internal, "Operation yielded unexpected result")),
        };
    }

    public override async Task<GetAnalysisResultCodeBlocksResponse> GetAnalysisResultCodeBlocks(
        GetAnalysisResultCodeBlocksRequest request,
        ServerCallContext context)
    {
        var applicationRequest = new GetResultCodeBlocks.Request(
            request.AnalysisId.MapToAnalysisId(),
            request.FirstSubmissionId.MapToGuid(),
            request.SecondSubmissionId.MapToGuid(),
            request.MinimumSimilarityScore,
            request.Cursor);

        GetResultCodeBlocks.Response applicationResponse = await _service
            .GetAnalysisResultCodeBlocksAsync(applicationRequest, context.CancellationToken);

        return applicationResponse switch
        {
            GetResultCodeBlocks.Response.Success success => new GetAnalysisResultCodeBlocksResponse
            {
                Success = new GetAnalysisResultCodeBlocksResponse.Types.Success
                {
                    CodeBlocks = { success.CodeBlocks.Select(x => x.MapToGrpcModel()) },
                },
            },

            GetResultCodeBlocks.Response.AnalysisNotFound => new GetAnalysisResultCodeBlocksResponse
            {
                AnalysisNotFound = new(),
            },

            _ => throw new RpcException(new Status(StatusCode.Internal, "Operation yielded unexpected result")),
        };
    }
}