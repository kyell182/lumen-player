using System.ComponentModel;
using System.Diagnostics;

namespace Lumen.Infrastructure;

/// <summary>Opent een link in de standaardbrowser.</summary>
public static class ExternalLauncher
{
    public static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Win32Exception ex)
        {
            CrashLogger.Write($"Kon {url} niet openen", ex);
        }
    }
}
