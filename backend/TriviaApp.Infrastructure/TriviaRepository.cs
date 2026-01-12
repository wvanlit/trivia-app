using System.Text.Json;
using Dapper;
using Npgsql;
using TriviaApp.Domain.Model;
using TriviaApp.Domain.Ports;

namespace TriviaApp.Infrastructure;

public sealed class TriviaRepository(NpgsqlDataSource dataSource) : ITriviaRepository
{
    /// <summary>
    /// Clears trivia data so ingestion can repopulate from a known baseline.
    /// </summary>
    public async Task Truncate(CancellationToken cancellationToken)
    {
        const string sql = """
            TRUNCATE TABLE questions, category RESTART IDENTITY CASCADE;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Inserts or updates a category while preserving the source identifier.
    /// </summary>
    public async Task<Category> UpsertCategory(Category category, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO category (category_id, name)
            VALUES (@CategoryId, @Name)
            ON CONFLICT (category_id)
            DO UPDATE SET name = EXCLUDED.name
            RETURNING category_id;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var categoryId = await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(
                sql,
                new { CategoryId = category.Id.ToInt64(), category.Name },
                cancellationToken: cancellationToken));
        return new Category(CategoryId.FromInt64(categoryId), category.Name);
    }

    /// <summary>
    /// Persists a batch of trivia questions.
    /// </summary>
    public async Task<int> InsertQuestions(
        CategoryId categoryId,
        IReadOnlyList<NewQuestion> questions,
        CancellationToken cancellationToken)
    {
        if (questions.Count == 0)
        {
            return 0;
        }

        const string sql = """
            INSERT INTO questions (category_id, question, correct_answer, options, difficulty)
            VALUES (@CategoryId, @Question, @CorrectAnswer, CAST(@Options AS jsonb), @Difficulty)
            """;

        var parameters = questions.Select(question => new
        {
            CategoryId = (long)categoryId,
            Question = question.Text,
            CorrectAnswer = (string)question.CorrectAnswer,
            Options = JsonSerializer.Serialize(question.Options.Select(answer => (string)answer).ToArray()),
            Difficulty = question.Difficulty.Value
        });

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

}
