namespace ProdTrack.E2E.Tests;

/// <summary>Downloads Playwright's Chromium into the user profile on first use (no admin rights, no system install).</summary>
internal static class PlaywrightInstaller
{
    private static readonly Lock Gate = new();
    private static bool _installed;

    public static void EnsureChromium()
    {
        lock (Gate)
        {
            if (_installed)
            {
                return;
            }

            var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
            if (exitCode != 0)
            {
                throw new InvalidOperationException($"Playwright could not install Chromium (exit code {exitCode}). See README 'End-to-end tests'.");
            }

            _installed = true;
        }
    }
}
