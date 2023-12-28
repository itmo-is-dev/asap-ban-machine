using FluentMigrator;
using Itmo.Dev.Platform.Postgres.Migrations;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.Persistence.Migrations;

#pragma warning disable SA1649

[Migration(1703782577, "Initial")]
public class Initial : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider) =>
    """
    create type code_block as
    (
        code_block_file_path text ,
        code_block_line_from int ,
        code_block_line_to int ,
        code_block_content text
    );    

    create table analysis
    (
        analysis_id bigint primary key generated always as identity ,
        analysis_created_at timestamp not null 
    );

    create table analysis_data
    (
        analysis_id bigint not null references analysis(analysis_id),
        analysis_data_submission_id uuid not null ,
        analysis_data_user_id uuid not null ,
        analysis_data_assignment_id uuid not null ,
        analysis_data_file_link uuid not null ,
        
        primary key (analysis_id, analysis_data_submission_id)
    );

    create table analysis_result_data
    (
        analysis_id bigint not null references analysis(analysis_id),
        analysis_result_data_first_submission_id uuid not null ,
        analysis_result_data_second_submission_id uuid not null ,
        analysis_result_data_similarity_score float8 not null ,
        
        primary key (analysis_id, analysis_result_data_first_submission_id, analysis_result_data_second_submission_id)
    );

    create table analysis_result_code_blocks
    (
        analysis_result_code_block_id bigint primary key generated always as identity ,
        analysis_id bigint not null references analysis(analysis_id),
        analysis_result_code_block_first_submission_id uuid not null ,
        analysis_result_code_block_second_submission_id uuid not null ,
        analysis_result_code_block_first code_block not null ,
        analysis_result_code_block_second code_block not null ,
        analysis_result_code_block_similarity_score float8 not null 
    );
    """;

    protected override string GetDownSql(IServiceProvider serviceProvider) =>
    """
    drop table analysis;
    drop table analysis_data;
    drop table analysis_result_data;
    drop table analysis_result_code_blocks;

    drop type code_block;
    """;
}