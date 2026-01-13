using System.Net.Http.Json;
using Xunit;

namespace TriviaApp.Tests.Api.Integration;

[CollectionDefinition("Api collection")]
public sealed class ApiTestSuite : ICollectionFixture<ApiTestFixture>;

[Collection("Api collection")]
public abstract class ApiTestBase(ApiTestFixture fixture) : IAsyncLifetime
{
    protected static IReadOnlyList<string> DefaultOptions => ["A", "B", "C", "D"];

    protected const long NerdCultureCategoryId = 1;
    protected const long ProgrammingCategoryId = 10;
    protected const long CoffeeCategoryId = 20;
    protected const long ScienceFictionCategoryId = 5;
    protected const long LiteratureCategoryId = 30;

    private readonly ApiTestFixture _fixture = fixture;

    protected ApiTestHarness Harness { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Harness = _fixture.Harness;
        await Harness.ResetDatabase();
        await Harness.InsertCategory(NerdCultureCategoryId, "Nerd Culture");
        await Harness.InsertCategory(ProgrammingCategoryId, "Programming");
        await Harness.InsertCategory(CoffeeCategoryId, "Coffee");
        await Harness.InsertCategory(ScienceFictionCategoryId, "Science Fiction");
        await Harness.InsertCategory(LiteratureCategoryId, "Literature");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected Task<long[]> SeedQuestions(
        long categoryId,
        string prefix,
        string correctAnswer,
        int count,
        string difficulty)
    {
        var insertions = Enumerable.Range(0, count)
            .Select(index => Harness.InsertQuestion(
                categoryId,
                $"{prefix} {index}",
                correctAnswer,
                DefaultOptions,
                difficulty));

        return Task.WhenAll(insertions);
    }

    protected Task<HttpResponseMessage> VerifyAnswer(long questionId, int selectedOptionIndex) =>
        Harness.Client.PostAsJsonAsync(
            new Uri("/api/questions/verify", UriKind.Relative),
            new { QuestionId = questionId, SelectedOptionIndex = selectedOptionIndex });
}

public sealed class ApiTestFixture : IAsyncLifetime
{
    internal ApiTestHarness Harness { get; private set; } = null!;

    async Task IAsyncLifetime.InitializeAsync() => Harness = await ApiTestHarness.Create();

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (Harness is not null)
        {
            await Harness.DisposeAsync();
        }
    }
}
