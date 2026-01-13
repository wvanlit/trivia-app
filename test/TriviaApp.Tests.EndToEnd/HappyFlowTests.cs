using FluentAssertions;
using Microsoft.Playwright;
using TriviaApp.Tests.EndToEnd.Pages;
using TriviaApp.Tests.EndToEnd.Setup;
using Xunit;

namespace TriviaApp.Tests.EndToEnd;

public sealed class HappyFlowTests(PlaywrightFixture fixture) : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture = fixture;

    [Fact]
    public async Task WhenCompletingQuizThenShowsExpectedAccuracy()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));

        await using var harness = await TestHarness.Create(cts.Token);
        await using var session = await _fixture.CreatePage();

        var page = session.Page;

        await page.GotoAsync(harness.FrontendBaseUrl.ToString());

        await Assertions.Expect(page.GetByTestId("start-screen")).ToBeVisibleAsync();

        var startScreen = new StartScreenPage(page);

        await startScreen.SelectQuestionCount(3);
        await startScreen.SelectCategory(TestData.CategoryName);
        await startScreen.StartQuiz();

        var quizScreen = new QuizScreenPage(page);

        foreach (var _ in TestData.Questions)
        {
            var questionText = await quizScreen.ReadQuestionText();

            var answerIndex = TestData.GetAnswerIndex(questionText);

            await quizScreen.AnswerOption(answerIndex);
            await quizScreen.WaitForNextQuestionOrResult(questionText);
        }

        await Assertions.Expect(page.GetByTestId("result-screen")).ToBeVisibleAsync();

        var resultScreen = new ResultScreenPage(page);
        var accuracy = await resultScreen.ReadAccuracy();

        accuracy.Should().Be("66%");
    }
}
