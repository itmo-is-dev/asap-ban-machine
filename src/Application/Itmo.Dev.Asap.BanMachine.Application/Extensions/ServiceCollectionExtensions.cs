using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.BanMachine.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        return collection;
    }
}