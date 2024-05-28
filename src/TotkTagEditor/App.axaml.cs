using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using TotkTagEditor.Views;

namespace TotkTagEditor;

public partial class App : Application
{
    public static readonly string AppName = "TotK Tag Editor";
    public static readonly string Version = typeof(App).Assembly.GetName().Version?.ToString(3) ?? "x.x.x";

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
            singleViewPlatform.MainView = new ShellView();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public Visual? GetRoot()
    {
        return ApplicationLifetime switch {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            ISingleViewApplicationLifetime singleViewPlatform => singleViewPlatform.MainView,
            _ => null
        };
    }
}
