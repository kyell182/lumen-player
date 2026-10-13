using Avalonia.Threading;

namespace Lumen.Infrastructure;

/// <summary>
/// Schrijft onverwachte fouten naar crash.log in de Lumen-gegevensmap.
/// Fouten op de UI-thread worden gelogd en opgevangen, zodat de app niet afsluit.
/// </summary>
public static class CrashLogger
{
    private static readonly object Gate = new();

    public static string LogPath { get; } = AppPaths.DataFile("crash.log");

    public static void Register()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Write("Onverwerkte fout (app sluit af)", e.ExceptionObject as Exception);

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Write("Niet-afgehandelde taakfout", e.Exception);
            e.SetObserved();
        };
    }

    /// <summary>Pas aanroepen nadat Avalonia is opgestart, anders bestaat de UI-thread nog niet.</summary>
    public static void CatchUiThreadErrors()
    {
        Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            Write("Fout op de UI-thread (opgevangen)", e.Exception);
            e.Handled = true;
        };
    }

    public static void Write(string context, Exception? exception)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(AppPaths.DataDirectory);
                File.AppendAllText(LogPath,
                    $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] {context}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}");
            }
        }
        catch (IOException)
        {
            // Loggen mag zelf nooit een crash veroorzaken.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
