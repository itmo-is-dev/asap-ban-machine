using Itmo.Dev.Asap.BanMachine.Application.Abstractions.Persistence.Queries;
using Itmo.Dev.Asap.BanMachine.Application.Abstractions.Persistence.Repositories;
using Itmo.Dev.Asap.BanMachine.Application.Models.Analysis;
using Itmo.Dev.Asap.BanMachine.Application.Models.Submissions;
using Itmo.Dev.Platform.Postgres.Connection;
using Itmo.Dev.Platform.Postgres.Extensions;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Asap.BanMachine.Infrastructure.Persistence.Repositories;

internal class AnalysisRepository : IAnalysisRepository
{
    private readonly IPostgresConnectionProvider _connectionProvider;

    public AnalysisRepository(IPostgresConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async IAsyncEnumerable<SubmissionDataPair> QueryDataPairsAsync(
        AnalysisDataQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        with current_analysis_data as
        (
            select *
            from analysis_data
            where analysis_id = :analysis_id
        )
        select f.analysis_data_assignment_id as assignment_id, 
               f.analysis_data_submission_id as first_submission_id, 
               f.analysis_data_user_id as fist_user_id, 
               f.analysis_data_file_link as first_file_link, 
               s.analysis_data_submission_id as second_submission_id, 
               s.analysis_data_user_id as second_user_id,
               s.analysis_data_file_link as second_file_link
        from current_analysis_data f
        join current_analysis_data s on 
            f.analysis_data_submission_id != s.analysis_data_submission_id 
            and f.analysis_data_assignment_id = s.analysis_data_assignment_id
        where 
            (:should_ignore_first_filter or f.analysis_data_submission_id >= :fist_submission_id)
            and 
            (
                :should_ignore_first_filter 
                or :should_ignore_second_filter
                or f.analysis_data_submission_id != :fist_submission_id 
                or s.analysis_data_submission_id > :second_submission_id
            )
        order by f.analysis_data_submission_id, s.analysis_data_submission_id
        limit :page_size;
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("analysis_id", query.AnalysisId.Value)
            .AddParameter("should_ignore_first_filter", query.FirstSubmissionIdCursor is null)
            .AddParameter("should_ignore_second_filter", query.SecondSubmissionIdCursor is null)
            .AddParameter("fist_submission_id", query.FirstSubmissionIdCursor ?? Guid.Empty)
            .AddParameter("second_submission_id", query.SecondSubmissionIdCursor ?? Guid.Empty)
            .AddParameter("page_size", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int assignmentId = reader.GetOrdinal("assignment_id");
        int fistSubmissionId = reader.GetOrdinal("first_submission_id");
        int fistUserId = reader.GetOrdinal("fist_user_id");
        int fistFileLink = reader.GetOrdinal("first_file_link");
        int secondSubmissionId = reader.GetOrdinal("second_submission_id");
        int secondUserId = reader.GetOrdinal("second_user_id");
        int secondFileLink = reader.GetOrdinal("second_file_link");

        while (await reader.ReadAsync(cancellationToken))
        {
            Guid assignmentIdValue = reader.GetGuid(assignmentId);

            var first = new SubmissionData(
                SubmissionId: reader.GetGuid(fistSubmissionId),
                UserId: reader.GetGuid(fistUserId),
                AssignmentId: assignmentIdValue,
                FileLink: reader.GetString(fistFileLink));

            var second = new SubmissionData(
                SubmissionId: reader.GetGuid(secondSubmissionId),
                UserId: reader.GetGuid(secondUserId),
                AssignmentId: assignmentIdValue,
                FileLink: reader.GetString(secondFileLink));

            yield return new SubmissionDataPair(first, second);
        }
    }

    public async IAsyncEnumerable<SubmissionPairAnalysisResultData> QueryAnalysisResultDataAsync(
        AnalysisResultDataQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        select analysis_result_data_first_submission_id, 
               analysis_result_data_second_submission_id, 
               analysis_result_data_similarity_score
        from analysis_result_data
        where
            analysis_id = :analysis_id
            and (:should_ignore_fist_filter or analysis_result_data_first_submission_id >= :fist_submission_id)
            and
            (
                :should_ignore_fist_filter 
                or :should_ignore_second_filter
                or analysis_result_data_first_submission_id != :fist_submission_id 
                or analysis_result_data_second_submission_id > :second_submission_id
            )
        order by analysis_result_data_first_submission_id, analysis_result_data_second_submission_id
        limit :page_size;
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("analysis_id", query.AnalysisId.Value)
            .AddParameter("should_ignore_fist_filter", query.FirstSubmissionId is null)
            .AddParameter("should_ignore_second_filter", query.SecondSubmissionId is null)
            .AddParameter("fist_submission_id", query.FirstSubmissionId ?? Guid.Empty)
            .AddParameter("second_submission_id", query.SecondSubmissionId ?? Guid.Empty);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int fistSubmissionId = reader.GetOrdinal("analysis_result_data_first_submission_id");
        int secondSubmissionId = reader.GetOrdinal("analysis_result_data_second_submission_id");
        int similarityScore = reader.GetOrdinal("analysis_result_data_similarity_score");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new SubmissionPairAnalysisResultData(
                FirstSubmissionId: reader.GetGuid(fistSubmissionId),
                SecondSubmissionId: reader.GetGuid(secondSubmissionId),
                SimilarityScore: reader.GetDouble(similarityScore));
        }
    }

    public async IAsyncEnumerable<SimilarCodeBlocks> QueryAnalysisResultCodeBlocks(
        AnalysisResultCodeBlocksQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        select analysis_result_code_block_first, 
               analysis_result_code_block_second,
               analysis_result_code_block_similarity_score
        from analysis_result_code_blocks
        where 
            analysis_id = :analysis_id
            and analysis_result_code_block_first_submission_id = :fist_submission_id
            and analysis_result_code_block_second_submission_id = :second_submission_id
            and analysis_result_code_block_similarity_score >= :minimum_similarity_score 
        order by analysis_result_code_block_id
        offset :cursor
        limit :page_size;
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("analysis_id", query.AnalysisId.Value)
            .AddParameter("fist_submission_id", query.FirstSubmissionId)
            .AddParameter("second_submission_id", query.SecondSubmissionId)
            .AddParameter("minimum_similarity_score", query.MinimumSimilarityScore)
            .AddParameter("cursor", query.Cursor)
            .AddParameter("page_size", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int fistBlock = reader.GetOrdinal("analysis_result_code_block_first");
        int secondBlock = reader.GetOrdinal("analysis_result_code_block_second");
        int similarityScore = reader.GetOrdinal("analysis_result_code_block_similarity_score");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new SimilarCodeBlocks(
                First: reader.GetFieldValue<CodeBlock>(fistBlock),
                Second: reader.GetFieldValue<CodeBlock>(secondBlock),
                SimilarityScore: reader.GetDouble(similarityScore));
        }
    }

    public async Task<AnalysisId> CreateAsync(CancellationToken cancellationToken)
    {
        const string sql = """
        insert into analysis(analysis_created_at) values (now())
        returning analysis_id;
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return new AnalysisId(reader.GetInt64(0));
    }

    public async Task AddDataAsync(
        AnalysisId analysisId,
        IReadOnlyCollection<SubmissionData> data,
        CancellationToken cancellationToken)
    {
        const string sql = """
        insert into analysis_data
        (analysis_id, analysis_data_submission_id, analysis_data_user_id, analysis_data_assignment_id, analysis_data_file_link)
        select :analysis_id, * 
        from unnest(:submission_ids, :user_ids, :assignment_ids, :file_links)
        on conflict do update set analysis_data_file_link = excluded.analysis_data_file_link;
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("analysis_id", analysisId.Value)
            .AddParameter("submission_ids", data.Select(x => x.SubmissionId).ToArray())
            .AddParameter("user_ids", data.Select(x => x.UserId).ToArray())
            .AddParameter("assignment_ids", data.Select(x => x.AssignmentId).ToArray())
            .AddParameter("file_links", data.Select(x => x.FileLink).ToArray());

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AddAnalysisResultAsync(
        AnalysisId analysisId,
        SubmissionPairAnalysisResult result,
        CancellationToken cancellationToken)
    {
        const string sql = """
        insert into analysis_result_data
        (analysis_id, analysis_result_data_first_submission_id, analysis_result_data_second_submission_id, analysis_result_data_similarity_score)
        values (:analysis_id, :fist_submission_id, :second_submission_id, :similarity_score);

        insert into analysis_result_code_blocks(analysis_id,
                                                analysis_result_code_block_first_submission_id,
                                                analysis_result_code_block_second_submission_id,
                                                analysis_result_code_block_first,
                                                analysis_result_code_block_second,
                                                analysis_result_code_block_similarity_score)
        select :analysis_id, :fist_submission_id, :second_submission_id, * 
        from unnest(:fist_code_blocks, :second_code_blocks, :similarity_scores);
        """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("analysis_id", analysisId.Value)
            .AddParameter("fist_submission_id", result.Data.FirstSubmissionId)
            .AddParameter("second_submission_id", result.Data.SecondSubmissionId)
            .AddParameter("similarity_score", result.Data.SimilarityScore)
            .AddParameter("fist_code_blocks", result.SimilarCodeBlocks.Select(x => x.First).ToArray())
            .AddParameter("second_code_blocks", result.SimilarCodeBlocks.Select(x => x.Second).ToArray())
            .AddParameter("similarity_scores", result.SimilarCodeBlocks.Select(x => x.SimilarityScore).ToArray());

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}