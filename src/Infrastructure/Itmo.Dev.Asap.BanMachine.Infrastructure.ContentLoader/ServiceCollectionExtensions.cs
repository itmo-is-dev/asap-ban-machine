using Itmo.Dev.Asap.BanMachine.Application.Abstractions.Submissions;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.ContentLoader;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureContentLoader(this IServiceCollection collection)
    {
        collection.AddHttpClient<SubmissionContentLoader>();
        collection.AddScoped<ISubmissionContentLoader>(p => p.GetRequiredService<SubmissionContentLoader>());

        return collection;
    }
}