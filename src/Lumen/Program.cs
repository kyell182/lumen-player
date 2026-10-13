using Avalonia;
using LibVLCSharp.Shared;
using Lumen.Infrastructure;

namespace Lumen;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        LinuxLibVlcResolver.Register();
        Core.Initialize();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Ook gebruikt door de XAML-previewer van je IDE.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
