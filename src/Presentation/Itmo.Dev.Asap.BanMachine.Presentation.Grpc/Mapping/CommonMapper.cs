using Grpc.Core;
using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;

namespace Itmo.Dev.Asap.BanMachine.Presentation.Grpc.Mapping;

internal static class CommonMapper
{
    public static Guid MapToGuid(this string value)
    {
        return Guid.TryParse(value, out Guid guid)
            ? guid
            : throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid guid format"));
    }

    public static AnalysisId MapToAnalysisId(this string value)
    {
        return long.TryParse(value, out long analysisId)
            ? new AnalysisId(analysisId)
            : throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid analysis id format"));
    }
}