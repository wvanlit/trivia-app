using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace TriviaApp.Tests.Api.Integration;

public sealed class VerifyAnswerEndpointTests(ApiTestFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task WhenVerifyingAnswerThenReturnsCorrectness()
    {
        var questionId = await Harness.InsertQuestion(ScienceFictionCategoryId, "Pick B", "B", DefaultOptions, "easy");

        var response = await VerifyAnswer(questionId, selectedOptionIndex: 1);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<VerifyAnswerResponse>();

        payload!.IsCorrect.Should().BeTrue();
    }

    [Fact]
    public async Task WhenVerifyingWrongAnswerThenReturnsIncorrect()
    {
        var questionId = await Harness.InsertQuestion(ScienceFictionCategoryId, "Pick B", "B", DefaultOptions, "easy");

        var response = await VerifyAnswer(questionId, selectedOptionIndex: 0);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<VerifyAnswerResponse>();

        payload!.IsCorrect.Should().BeFalse();
    }

    [Fact]
    public async Task WhenVerifyingUnknownQuestionThenReturnsBadRequest()
    {
        var response = await VerifyAnswer(questionId: 999L, selectedOptionIndex: 0);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WhenVerifyingAnswerWithInvalidOptionIndexThenReturnsBadRequest()
    {
        var questionId = await Harness.InsertQuestion(ScienceFictionCategoryId, "Pick B", "B", DefaultOptions, "easy");

        var response = await VerifyAnswer(questionId, selectedOptionIndex: 4);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WhenVerifyingAnswerWithNegativeOptionIndexThenReturnsBadRequest()
    {
        var questionId = await Harness.InsertQuestion(ScienceFictionCategoryId, "Pick B", "B", DefaultOptions, "easy");

        var response = await VerifyAnswer(questionId, selectedOptionIndex: -1);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [SuppressMessage(
        "Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "System.Text.Json materializes DTOs during deserialization.")]
    private sealed record VerifyAnswerResponse(bool IsCorrect);
}
