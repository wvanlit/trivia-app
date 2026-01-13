using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TriviaApp.Domain.Configuration;
using TriviaApp.Domain.Model;
using TriviaApp.Domain.Workflows;
using TriviaApp.Tests.Shared;
using Xunit;

namespace TriviaApp.Tests.Worker.Integration;

public sealed class TriviaIngestionWorkflowTests(WorkerTestFixture fixture) : WorkerTestBase(fixture)
{
    private readonly Category _nerdCultureCategory = new(1, "Nerd Culture");
    private readonly Category _programmingCategory = new(2, "Programming");
    private readonly Category _literatureCategory = new(99, "Literature");

    [Fact]
    public async Task WhenRunThenTruncatesExistingTriviaData()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        await SeedLiteratureCategoryWithQuestion(cancellationToken);

        var workflow = CreateWorkflow([]);

        // Act
        await workflow.Run(cancellationToken);

        // Assert
        (await Harness.CountCategories(cancellationToken)).Should().Be(0);
        (await Harness.CountQuestionsInCategory(_literatureCategory.Id, cancellationToken)).Should().Be(0);
    }

    [Fact]
    public async Task WhenRunThenPersistsDataFromTriviaSource()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var categoryQuestions = new Dictionary<Category, int>
        {
            [_nerdCultureCategory] = 6,
            [_programmingCategory] = 3
        };

        var workflow = CreateWorkflow(categoryQuestions);

        // Act
        await workflow.Run(cancellationToken);

        // Assert
        (await Harness.CountCategories(cancellationToken)).Should().Be(2);
        (await Harness.CountQuestions(cancellationToken)).Should().Be(9);
        (await Harness.CountQuestionsInCategory(_nerdCultureCategory.Id, cancellationToken)).Should().Be(6);
        (await Harness.CountQuestionsInCategory(_programmingCategory.Id, cancellationToken)).Should().Be(3);
    }

    [Fact]
    public async Task WhenRunThenPersistsQuestionsUpToMaxPerCategory()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var categoryQuestions = new Dictionary<Category, int>
        {
            [_nerdCultureCategory] = 6,
            [_programmingCategory] = 3
        };

        var workflow = CreateWorkflow(categoryQuestions, maxQuestionsPerCategory: 5);

        // Act
        await workflow.Run(cancellationToken);

        // Assert
        (await Harness.CountCategories(cancellationToken)).Should().Be(2);
        (await Harness.CountQuestions(cancellationToken)).Should().Be(8);
        (await Harness.CountQuestionsInCategory(_nerdCultureCategory.Id, cancellationToken)).Should().Be(5);
        (await Harness.CountQuestionsInCategory(_programmingCategory.Id, cancellationToken)).Should().Be(3);
    }

    private async Task SeedLiteratureCategoryWithQuestion(CancellationToken cancellationToken)
    {
        var literature = await Harness.Repository.UpsertCategory(_literatureCategory, cancellationToken);
        await Harness.Repository.InsertQuestions(
            literature.Id,
            [TestQuestionFactory.CreateQuestion("Literature")],
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
            Harness.Repository,
            options,
            NullLogger<TriviaIngestionWorkflow>.Instance);
    }
}
