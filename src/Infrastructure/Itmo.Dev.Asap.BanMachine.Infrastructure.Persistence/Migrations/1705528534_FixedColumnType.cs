using FluentMigrator;
using Itmo.Dev.Platform.Postgres.Migrations;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.Persistence.Migrations;

#pragma warning disable SA1649

[Migration(1705528534, "Fixed analysis_data_file_link")]
public class FixedColumnType : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider) =>
    """
    alter table analysis_data
        alter column analysis_data_file_link type text;
    """;

    protected override string GetDownSql(IServiceProvider serviceProvider) =>
    """
    alter table analysis_data
        alter column analysis_data_file_link type uuid;
    """;
}