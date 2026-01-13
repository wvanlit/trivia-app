using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TriviaApp.Domain.Configuration;
using TriviaApp.Domain.Model;
using TriviaApp.Domain.Workflows;
using TriviaApp.Tests.Shared;
using Xunit;

namespace TriviaApp.Tests.Worker.Integration;

public sealed class TriviaIngestionWorkflowTests : IAsyncLifetime
{
    private readonly Category _historyCategory = new(1, "History");
    private readonly Category _scienceCategory = new(2, "Science"); private readonly Category _randomCategory = new(99, "Random");

    private TestHarness _harness = null!; // Always initialized by InitializeAsync

    public async Task InitializeAsync() => _harness = await TestHarness.Create();

    public async Task DisposeAsync()
    {
        if (_harness is not null)
        {
            await _harness.DisposeAsync();
        }
    }

    [Fact]
    public async Task WhenRunThenTruncatesExistingTriviaData()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        await SeedRandomCategoryWithQuestion(cancellationToken);

        var workflow = CreateWorkflow([]);

        // Act
        await workflow.Run(cancellationToken);

        // Assert
        (await _harness.CountCategories(cancellationToken)).Should().Be(0);
        (await _harness.CountQuestionsInCategory(_randomCategory.Id, cancellationToken)).Should().Be(0);
    }

    [Fact]
    public async Task WhenRunThenPersistsDataFromTriviaSource()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var categoryQuestions = new Dictionary<Category, int>
        {
            [_historyCategory] = 6,
            [_scienceCategory] = 3
        };

        var workflow = CreateWorkflow(categoryQuestions);

        // Act
        await workflow.Run(cancellationToken);

        // Assert
        (await _harness.CountCategories(cancellationToken)).Should().Be(2);
        (await _harness.CountQuestions(cancellationToken)).Should().Be(9);
        (await _harness.CountQuestionsInCategory(_historyCategory.Id, cancellationToken)).Should().Be(6);
        (await _harness.CountQuestionsInCategory(_scienceCategory.Id, cancellationToken)).Should().Be(3);
    }

    [Fact]
    public async Task WhenRunThenPersistsQuestionsUpToMaxPerCategory()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var categoryQuestions = new Dictionary<Category, int>
        {
            [_historyCategory] = 6,
            [_scienceCategory] = 3
        };

        var workflow = CreateWorkflow(categoryQuestions, maxQuestionsPerCategory: 5);

        // Act
        await workflow.Run(cancellationToken);

        // Assert
        (await _harness.CountCategories(cancellationToken)).Should().Be(2);
        (await _harness.CountQuestions(cancellationToken)).Should().Be(8);
        (await _harness.CountQuestionsInCategory(_historyCategory.Id, cancellationToken)).Should().Be(5);
        (await _harness.CountQuestionsInCategory(_scienceCategory.Id, cancellationToken)).Should().Be(3);
    }

    private async Task SeedRandomCategoryWithQuestion(CancellationToken cancellationToken)
    {
        var random = await _harness.Repository.UpsertCategory(_randomCategory, cancellationToken);
        await _harness.Repository.InsertQuestions(
            random.Id,
            [TestQuestionFactory.CreateQuestion("Random")],
            cancellationToken);
    }

    private TriviaIngestionWorkflow CreateWorkflow(
        Dictionary<Category, int> categoryQuestions,
        int maxQuestionsPerCategory = 100)
    {
        var source = new FakeTriviaSource(
            categoryQuestions.Keys.ToList(),
            categoryQuestions.ToDictionary(entry => entry.Key.Id, entry => entry.Value));

        var options = new OptionsSnapshot<TriviaIngestionOptions>(
            new TriviaIngestionOptions
            {
                MaxQuestionsPerCategory = maxQuestionsPerCategory
            });

        return new TriviaIngestionWorkflow(
            source,
            _harness.Repository,
            options,
            NullLogger<TriviaIngestionWorkflow>.Instance);
    }
}
