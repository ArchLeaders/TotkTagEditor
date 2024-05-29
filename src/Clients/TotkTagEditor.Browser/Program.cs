using Avalonia;
using Avalonia.Browser;
using FluentIcons.Avalonia.Fluent;
using System.Runtime.Versioning;
using TotkTagEditor;

[assembly: SupportedOSPlatform("browser")]

internal partial class Program
{
    private static async Task Main()
    {
        await BuildAvaloniaApp()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
                .UseSegoeMetrics()
                .WithInterFont();
    }
}
