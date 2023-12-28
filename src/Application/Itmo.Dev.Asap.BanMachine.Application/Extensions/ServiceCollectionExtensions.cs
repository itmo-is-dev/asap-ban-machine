using Itmo.Dev.Asap.BanMachine.Application.Analysis;
using Itmo.Dev.Asap.BanMachine.Application.AnalysisResults;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.BanMachine.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        collection
            .AddOptions<AnalysisTaskOptions>()
            .BindConfiguration("Application:Analysis:BackgroundTask");

        collection
            .AddOptions<AnalysisResultsServiceOptions>()
            .BindConfiguration("Application:AnalysisResults");

        return collection;
    }
}