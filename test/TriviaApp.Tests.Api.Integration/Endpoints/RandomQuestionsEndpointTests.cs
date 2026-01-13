using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace TriviaApp.Tests.Api.Integration;

public sealed class RandomQuestionsEndpointTests(ApiTestFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task WhenRequestingRandomQuestionsWithoutCountThenReturnsFive()
    {
        await SeedQuestions(NerdCultureCategoryId, "Nerd Culture", "A", 7, "easy");

        var response = await Harness.Client.GetAsync(new Uri("/api/questions", UriKind.Relative));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<RandomQuestionsResponse>();

        payload!.Questions.Count.Should().Be(5);
    }

    [Fact]
    public async Task WhenFilteringByCategoryThenAllResultsMatchCategory()
    {
        await SeedQuestions(ProgrammingCategoryId, "Programming", "A", 3, "medium");
        await SeedQuestions(CoffeeCategoryId, "Coffee", "A", 3, "medium");

        var response = await Harness.Client.GetAsync(new Uri("/api/questions?count=2&categoryId=10", UriKind.Relative));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<RandomQuestionsResponse>();

        payload!.Questions.Select(question => question.CategoryId).Should().OnlyContain(id => id == ProgrammingCategoryId);
    }

    [Fact]
    public async Task WhenFilteringByUnknownCategoryThenReturnsBadRequest()
    {
        var response = await Harness.Client.GetAsync(new Uri("/api/questions?count=2&categoryId=999", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WhenRequestingRandomQuestionsWithInvalidCountThenReturnsBadRequest()
    {
        var response = await Harness.Client.GetAsync(new Uri("/api/questions?count=0", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WhenRequestingRandomQuestionsWithCountAboveLimitThenReturnsBadRequest()
    {
        var response = await Harness.Client.GetAsync(new Uri("/api/questions?count=11", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "System.Text.Json materializes DTOs during deserialization.")]
    private sealed record RandomQuestionsResponse(IReadOnlyList<QuestionResponse> Questions);

    [SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "System.Text.Json materializes DTOs during deserialization.")]
    private sealed record QuestionResponse(long QuestionId, long CategoryId, string Text, IReadOnlyList<string> Options, string Difficulty);
}
