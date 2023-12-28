using Itmo.Dev.Asap.BanMachine.Presentation.Grpc.Controllers;
using Microsoft.AspNetCore.Builder;

namespace Itmo.Dev.Asap.BanMachine.Presentation.Grpc.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGrpcPresentation(this IApplicationBuilder builder)
    {
        builder.UseEndpoints(x =>
        {
            x.MapGrpcService<AnalysisController>();
            x.MapGrpcService<AnalysisResultsController>();

            x.MapGrpcReflectionService();
        });

        return builder;
    }
}