using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection collection)
    {
        return collection;
    }
}