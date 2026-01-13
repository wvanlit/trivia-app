using Microsoft.Playwright;
using Xunit;

namespace TriviaApp.Tests.EndToEnd.Setup;

public sealed class PlaywrightFixture : IAsyncLifetime
{
    private static bool IsHeadless => true; // Toggle to enable a window when debugging tests
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public PlaywrightFixture()
    {
    }

    internal async Task<PlaywrightPage> CreatePage()
    {
        if (_browser is null)
        {
            throw new InvalidOperationException("Playwright browser has not been initialized.");
        }

        var context = await _browser.NewContextAsync();
        var page = await context.NewPageAsync();

        return new PlaywrightPage(context, page);
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();

        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = IsHeadless
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }

        _playwright?.Dispose();
    }
}

internal sealed class PlaywrightPage : IAsyncDisposable
{
    private readonly IBrowserContext _context;

    internal IPage Page { get; }

    internal PlaywrightPage(IBrowserContext context, IPage page)
    {
        _context = context;
        Page = page;
    }

    async ValueTask IAsyncDisposable.DisposeAsync() => await _context.DisposeAsync();
}
