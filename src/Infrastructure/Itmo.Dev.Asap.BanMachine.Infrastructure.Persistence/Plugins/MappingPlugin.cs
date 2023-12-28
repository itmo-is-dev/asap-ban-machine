using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;
using Itmo.Dev.Platform.Postgres.Plugins;
using Npgsql;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.Persistence.Plugins;

public class MappingPlugin : IDataSourcePlugin
{
    public void Configure(NpgsqlDataSourceBuilder builder)
    {
        builder.MapComposite<CodeBlock>();
    }
}