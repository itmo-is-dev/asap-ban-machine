using FluentMigrator;
using Itmo.Dev.Platform.Postgres.Migrations;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.Persistence.Migrations;

#pragma warning disable SA1649

[Migration(1709047604, "Changed similar code blocks")]
public class ChangedSimilarCodeBlocks : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider) =>
    """
    alter table analysis_result_code_blocks
    alter column analysis_result_code_block_first type code_block[] using ARRAY[analysis_result_code_block_first];
    
    alter table analysis_result_code_blocks
    alter column analysis_result_code_block_second type code_block[] using ARRAY[analysis_result_code_block_second];
    """;

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }
}