using Itmo.Dev.Asap.BanMachine.Application.Abstractions.BanMachine;
using Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Options;
using Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureMachineLearning(this IServiceCollection collection)
    {
        collection.AddOptions<MachineLearningOptions>().BindConfiguration("Infrastructure:ML");

        collection.AddScoped<IBanMachineService, BanMachineService>();
        collection.AddHostedService<PythonRestoreBackgroundService>();

        return collection;
    }
}