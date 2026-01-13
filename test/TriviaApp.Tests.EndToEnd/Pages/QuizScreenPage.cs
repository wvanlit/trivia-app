using Microsoft.Playwright;

namespace TriviaApp.Tests.EndToEnd.Pages;

internal sealed class QuizScreenPage(IPage page)
{
    private ILocator QuestionHeading => page.GetByTestId("question-card").GetByRole(AriaRole.Heading);

    internal async Task<string> ReadQuestionText()
    {
        var text = await QuestionHeading.TextContentAsync();
        return string.IsNullOrWhiteSpace(text)
            ? throw new InvalidOperationException("Question text was not available.")
            : text.Trim();
    }

    internal Task AnswerOption(int index) => page.GetByTestId($"answer-option-{index}").ClickAsync();

    internal async Task WaitForNextQuestionOrResult(string previousQuestionText)
    {
        var resultScreen = page.GetByTestId("result-screen");
        var deadline = DateTime.UtcNow.AddSeconds(30);

        // Poll the page to check the state
        // Not ideal, but the alternative was an inline JS script which prevents typesafety/compiler checks
        while (DateTime.UtcNow < deadline)
        {
            if (await resultScreen.IsVisibleAsync())
            {
                await Assertions.Expect(resultScreen).ToBeVisibleAsync();
                return;
            }

            var headingText = await QuestionHeading.TextContentAsync();
            if (!string.IsNullOrWhiteSpace(headingText) &&
                !string.Equals(headingText.Trim(), previousQuestionText, StringComparison.Ordinal))
            {
                await Assertions.Expect(QuestionHeading).Not.ToHaveTextAsync(previousQuestionText);
                return;
            }

            await page.WaitForTimeoutAsync(100);
        }

        throw new TimeoutException("Timed out waiting for the next question or the result screen.");
    }
}
