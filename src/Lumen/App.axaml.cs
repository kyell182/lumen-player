using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lumen.Composition;
using Lumen.Features.Shell;
using Lumen.Infrastructure;

namespace Lumen;

public partial class App : Application
{
    private AppServices? _services;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        CrashLogger.CatchUiThreadErrors();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            StartDesktop(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void StartDesktop(IClassicDesktopStyleApplicationLifetime desktop)
    {
        MainWindow? window = null;
        _services = new AppServices(() => window);
        _services.Theme.ApplySaved();
        _services.Skins.ApplySaved();

        var viewModel = _services.CreateMainViewModel();
        window = new MainWindow(viewModel, _services.VideoOutput);
        desktop.MainWindow = window;

        var startupArgs = desktop.Args ?? Array.Empty<string>();
        window.Opened += (_, _) =>
        {
            viewModel.OpenStartupFiles(startupArgs);
            _ = viewModel.Update.CheckOnStartupAsync();
        };
        desktop.Exit += (_, _) => _services.Dispose();
    }
}
