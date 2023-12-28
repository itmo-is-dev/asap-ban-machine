using Itmo.Dev.Asap.BanMachine.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.BanMachine.Infrastructure.Persistence.Migrations;
using Itmo.Dev.Asap.BanMachine.Infrastructure.Persistence.Plugins;
using Itmo.Dev.Asap.BanMachine.Infrastructure.Persistence.Repositories;
using Itmo.Dev.Platform.Postgres.Extensions;
using Itmo.Dev.Platform.Postgres.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection collection)
    {
        collection.AddPlatformPostgres(builder => builder.BindConfiguration("Infrastructure:Persistence:Postgres"));
        collection.AddSingleton<IDataSourcePlugin, MappingPlugin>();

        collection.AddPlatformMigrations(typeof(IAssemblyMarker).Assembly);
        collection.AddHostedService<MigrationRunnerService>();

        collection.AddScoped<IAnalysisRepository, AnalysisRepository>();

        return collection;
    }
}