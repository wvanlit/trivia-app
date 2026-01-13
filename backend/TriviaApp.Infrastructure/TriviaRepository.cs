using System.Text.Json;
using Dapper;
using Npgsql;
using TriviaApp.Domain.Model;
using TriviaApp.Domain.Ports;

namespace TriviaApp.Infrastructure;

public sealed class TriviaRepository(NpgsqlDataSource dataSource) : ITriviaRepository
{
    public async Task Truncate(CancellationToken cancellationToken)
    {
        const string sql = """
            TRUNCATE TABLE questions, category RESTART IDENTITY CASCADE;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

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

    public async Task<IReadOnlyList<Question>> GetRandomQuestions(
        int count,
        CategoryId? categoryId,
        CancellationToken cancellationToken)
    {
        const string sqlAll = """
            SELECT
                question_id AS QuestionId,
                category_id AS CategoryId,
                question AS Question,
                correct_answer AS CorrectAnswer,
                options::text AS Options,
                difficulty AS Difficulty
            FROM questions
            ORDER BY RANDOM()
            LIMIT @Count;
            """;

        const string sqlByCategory = """
            SELECT
                question_id AS QuestionId,
                category_id AS CategoryId,
                question AS Question,
                correct_answer AS CorrectAnswer,
                options::text AS Options,
                difficulty AS Difficulty
            FROM questions
            WHERE category_id = @CategoryId
            ORDER BY RANDOM()
            LIMIT @Count;
            """;

        var sql = categoryId is null ? sqlAll : sqlByCategory;
        var parameters = new
        {
            Count = count,
            CategoryId = categoryId?.ToInt64()
        };

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var records = await connection.QueryAsync<QuestionRecord>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return records.Select(MapQuestion).ToArray();
    }

    public async Task<IReadOnlyList<Category>> GetCategories(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT category_id AS CategoryId, name AS Name
            FROM category
            ORDER BY name;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var records = await connection.QueryAsync<CategoryRecord>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return records
            .Select(record => new Category(CategoryId.FromInt64(record.CategoryId), record.Name))
            .ToArray();
    }

    public async Task<Question?> GetQuestion(QuestionId questionId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                question_id AS QuestionId,
                category_id AS CategoryId,
                question AS Question,
                correct_answer AS CorrectAnswer,
                options::text AS Options,
                difficulty AS Difficulty
            FROM questions
            WHERE question_id = @QuestionId;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var record = await connection.QuerySingleOrDefaultAsync<QuestionRecord>(
            new CommandDefinition(sql, new { QuestionId = questionId.ToInt64() }, cancellationToken: cancellationToken));

        return record is null ? null : MapQuestion(record);
    }

    public async Task<bool> CategoryExists(CategoryId categoryId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM category
                WHERE category_id = @CategoryId
            );
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { CategoryId = categoryId.ToInt64() }, cancellationToken: cancellationToken));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Dapper materializes records via reflection.")]
    private sealed record QuestionRecord(
        long QuestionId,
        long CategoryId,
        string Question,
        string CorrectAnswer,
        string Options,
        string Difficulty);

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Dapper materializes records via reflection.")]
    private sealed record CategoryRecord(long CategoryId, string Name);

    private static Question MapQuestion(QuestionRecord record)
    {
        var options = JsonSerializer.Deserialize<string[]>(record.Options) ?? [];
        var answers = options.Select(Answer.FromString).ToArray();

        return new Question(
            QuestionId.FromInt64(record.QuestionId),
            CategoryId.FromInt64(record.CategoryId),
            record.Question,
            Answer.FromString(record.CorrectAnswer),
            answers,
            Difficulty.FromString(record.Difficulty));
    }
}
