using FluentMigrator;
using Itmo.Dev.Platform.Postgres.Migrations;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.Persistence.Migrations;

#pragma warning disable SA1649

[Migration(1709073305, "Fixed code_block type")]
public class FixedCodeBlockType : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider) =>
    """
    alter type code_block
    rename attribute code_block_file_path to file_path,
    rename attribute code_block_line_from to line_from,
    rename attribute code_block_line_to to line_to,
    rename attribute code_block_content to content;
    """;

    protected override string GetDownSql(IServiceProvider serviceProvider) =>
    """
    alter type code_block
    rename attribute file_path to code_block_file_path,
    rename attribute line_from to code_block_line_from,
    rename attribute line_to to code_block_line_to,
    rename attribute content to code_block_content;
    """;
}