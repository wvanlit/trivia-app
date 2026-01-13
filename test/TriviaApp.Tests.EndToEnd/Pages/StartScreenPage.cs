using System.Globalization;
using Microsoft.Playwright;

namespace TriviaApp.Tests.EndToEnd.Pages;

internal sealed class StartScreenPage(IPage page)
{
    private ILocator QuestionCountSelect => page.GetByTestId("question-count-select");
    private ILocator CategorySelect => page.GetByTestId("category-select");
    private ILocator StartQuizButton => page.GetByTestId("start-quiz-button");

    internal async Task SelectQuestionCount(int count)
    {
        await QuestionCountSelect.ClickAsync();

        await page.GetByRole(AriaRole.Option, new() { Name = count.ToString(CultureInfo.InvariantCulture) }).ClickAsync();
    }

    internal async Task SelectCategory(string name)
    {
        await CategorySelect.ClickAsync();

        await page.GetByRole(AriaRole.Option, new() { Name = name }).ClickAsync();
    }

    internal Task StartQuiz() => StartQuizButton.ClickAsync();
}
