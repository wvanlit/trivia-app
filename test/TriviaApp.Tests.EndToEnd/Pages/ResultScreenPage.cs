using Microsoft.Playwright;

namespace TriviaApp.Tests.EndToEnd.Pages;

internal sealed class ResultScreenPage(IPage page)
{
    private ILocator AccuracyValue => page.GetByTestId("result-accuracy");

    internal async Task<string> ReadAccuracy()
    {
        var text = await AccuracyValue.TextContentAsync();

        return string.IsNullOrWhiteSpace(text)
            ? throw new InvalidOperationException("Accuracy value was not available.")
            : text.Trim();
    }
}
