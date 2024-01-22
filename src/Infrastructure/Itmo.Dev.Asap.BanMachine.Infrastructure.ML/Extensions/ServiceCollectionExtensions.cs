using Itmo.Dev.Asap.BanMachine.Application.Abstractions.BanMachine;
using Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.ML.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureMachineLearning(this IServiceCollection collection)
    {
        collection.AddScoped<IBanMachineService, BanMachineService>();
        collection.AddHostedService<PythonRestoreBackgroundService>();

        return collection;
    }
}