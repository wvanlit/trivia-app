using TriviaApp.Domain.Model;
using TriviaApp.Domain.Ports;

namespace TriviaApp.Tests.Shared;

/// <summary>
/// Captures repository calls for workflow tests.
/// </summary>
public sealed class FakeTriviaRepository : ITriviaRepository
{
    private readonly List<string> _calls = [];
    private readonly List<Category> _upserted = [];
    private readonly List<(CategoryId CategoryId, IReadOnlyList<NewQuestion> Questions)> _inserts = [];

    public IReadOnlyList<string> Calls => _calls;

    public IReadOnlyList<Category> Upserted => _upserted;

    public IReadOnlyList<(CategoryId CategoryId, IReadOnlyList<NewQuestion> Questions)> Inserts => _inserts;

    public Task Truncate(CancellationToken cancellationToken)
    {
        _calls.Add("Truncate");
        return Task.CompletedTask;
    }

    public Task<Category> UpsertCategory(Category category, CancellationToken cancellationToken)
    {
        _calls.Add("Upsert");
        _upserted.Add(category);

        return Task.FromResult(category);
    }

    public Task<int> InsertQuestions(
        CategoryId categoryId,
        IReadOnlyList<NewQuestion> questions,
        CancellationToken cancellationToken)
    {
        _calls.Add("Insert");
        _inserts.Add((categoryId, questions));

        return Task.FromResult(questions.Count);
    }
}
