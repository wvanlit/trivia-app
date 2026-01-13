using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TriviaApp.Domain.Configuration;
using TriviaApp.Domain.Model;
using TriviaApp.Domain.Workflows;
using TriviaApp.Tests.Shared;
using Xunit;

namespace TriviaApp.Tests.Domain.Unit;

public sealed class TriviaIngestionWorkflowTests
{
    private readonly Category _historyCategory = new(1, "History");
    private readonly Category _scienceCategory = new(7, "Science");
    private readonly Category _sportsCategory = new(4, "Sports");

    [Fact]
    public async Task WhenRunThenTruncatesBeforeUpserting()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var categoryQuestions = new Dictionary<Category, int>
        {
            [_historyCategory] = 1
        };

        var (workflow, repository) = CreateWorkflow(
            categoryQuestions,
            maxQuestionsPerCategory: 10,
            (_, _) => [TestQuestionFactory.CreateQuestion("Q1")]);

        // Act
        await workflow.Run(cancellationToken);

        // Assert
        repository.Calls.Should().NotBeEmpty();
        repository.Calls[0].Should().Be("Truncate");
        repository.Upserted.Should().ContainSingle();
        repository.Inserts.Should().ContainSingle();
    }

    [Fact]
    public async Task WhenCategoryExceedsMaxThenBatchesInsertions()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var requested = new List<int>();

        var categoryQuestions = new Dictionary<Category, int>
        {
            [_scienceCategory] = 125
        };

        var (workflow, repository) = CreateWorkflow(
            categoryQuestions,
            maxQuestionsPerCategory: 120,
            (amount, _) =>
            {
                requested.Add(amount);
                return TestQuestionFactory.CreateQuestions(amount, "S");
            });

        // Act
        await workflow.Run(cancellationToken);

        // Assert
        requested.Should().Equal(50, 50, 20);
        repository.Inserts.Select(insert => insert.Questions.Count).Should().Equal(50, 50, 20);
    }

    [Fact]
    public async Task WhenSourceReturnsEmptyThenStopsInserting()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var requested = new List<int>();

        var categoryQuestions = new Dictionary<Category, int>
        {
            [_sportsCategory] = 5
        };

        var (workflow, repository) = CreateWorkflow(
            categoryQuestions,
            maxQuestionsPerCategory: 10,
            (amount, _) =>
            {
                requested.Add(amount);
                return [];
            });

        // Act
        await workflow.Run(cancellationToken);

        // Assert
        requested.Should().Equal(5);
        repository.Inserts.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenMultipleCategoriesThenProcessesEachIndependently()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var categoryQuestions = new Dictionary<Category, int>
        {
            [_historyCategory] = 2,
            [_scienceCategory] = 3
        };

        var (workflow, repository) = CreateWorkflow(
            categoryQuestions,
            maxQuestionsPerCategory: 10,
            (amount, categoryId) => TestQuestionFactory.CreateQuestions(amount, $"Q-{categoryId.ToInt64()}"));

        // Act
        await workflow.Run(cancellationToken);

        // Assert
        repository.Upserted.Should().HaveCount(2);
        repository.Inserts.Should().HaveCount(2);
        repository.Inserts.Select(insert => insert.Questions.Count).Should().Equal(2, 3);
    }

    [Fact]
    public async Task WhenCategoryCountIsZeroThenSkipsInserts()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var categoryQuestions = new Dictionary<Category, int>
        {
            [_historyCategory] = 0
        };

        var (workflow, repository) = CreateWorkflow(
            categoryQuestions,
            maxQuestionsPerCategory: 10);

        // Act
        await workflow.Run(cancellationToken);

        // Assert
        repository.Upserted.Should().ContainSingle();
        repository.Inserts.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenMaxQuestionsPerCategoryIsZeroThenSkipsInserts()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var categoryQuestions = new Dictionary<Category, int>
        {
            [_historyCategory] = 5
        };

        var (workflow, repository) = CreateWorkflow(
            categoryQuestions,
            maxQuestionsPerCategory: 0);

        // Act
        await workflow.Run(cancellationToken);

        // Assert
        repository.Upserted.Should().ContainSingle();
        repository.Inserts.Should().BeEmpty();
    }

    private static (TriviaIngestionWorkflow Workflow, FakeTriviaRepository Repository) CreateWorkflow(
        Dictionary<Category, int> categoryQuestions,
        int maxQuestionsPerCategory,
        Func<int, CategoryId, IReadOnlyList<NewQuestion>>? questionsFactory = null)
    {
        var source = new FakeTriviaSource(
            categoryQuestions.Keys.ToList(),
            categoryQuestions.ToDictionary(entry => entry.Key.Id, entry => entry.Value),
            questionsFactory);

        var repository = new FakeTriviaRepository();

        var options = new OptionsSnapshot<TriviaIngestionOptions>(
            new TriviaIngestionOptions
            {
                MaxQuestionsPerCategory = maxQuestionsPerCategory
            });

        var workflow = new TriviaIngestionWorkflow(
            source,
            repository,
            options,
            NullLogger<TriviaIngestionWorkflow>.Instance);

        return (workflow, repository);
    }
}
