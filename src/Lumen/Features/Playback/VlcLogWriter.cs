using LibVLCSharp.Shared;
using Lumen.Infrastructure;

namespace Lumen.Features.Playback;

/// <summary>Schrijft waarschuwingen en fouten van VLC naar vlc.log (handig bij problemen met bestanden).</summary>
public sealed class VlcLogWriter
{
    private readonly object _gate = new();
    private readonly string _path = AppPaths.DataFile("vlc.log");

    public void Attach(LibVLC libVlc)
    {
        Reset();
        libVlc.Log += OnLog;
    }

    private void OnLog(object? sender, LogEventArgs e)
    {
        if (e.Level < LogLevel.Warning) return;

        try
        {
            lock (_gate)
            {
                File.AppendAllText(_path, $"[{DateTime.Now:HH:mm:ss}] {e.Level} {e.Module}: {e.Message}{Environment.NewLine}");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Loggen mag het afspelen nooit hinderen.
        }
    }

    private void Reset()
    {
        try
        {
            Directory.CreateDirectory(AppPaths.DataDirectory);
            File.WriteAllText(_path, string.Empty);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }
}
