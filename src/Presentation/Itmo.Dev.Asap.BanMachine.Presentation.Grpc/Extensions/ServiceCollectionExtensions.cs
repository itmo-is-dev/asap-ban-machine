using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.BanMachine.Presentation.Grpc.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationGrpc(this IServiceCollection collection)
    {
        return collection;
    }
}