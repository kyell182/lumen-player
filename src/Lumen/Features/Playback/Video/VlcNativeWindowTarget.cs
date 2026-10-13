using LibVLCSharp.Shared;
using Lumen.Infrastructure;

namespace Lumen.Features.Playback.Video;

/// <summary>
/// Koppelt VLC aan een native venster van Lumen. Wordt er afgespeeld voordat het venster bestaat,
/// dan wacht het afspelen tot het venster er is — anders opent VLC een eigen venster.
/// Komt het venster er na <see cref="WindowTimeout"/> niet, dan wordt toch afgespeeld (vangnet).
/// </summary>
public sealed class VlcNativeWindowTarget : INativeVideoTarget
{
    private static readonly TimeSpan WindowTimeout = TimeSpan.FromSeconds(3);

    private readonly MediaPlayer _player;
    private readonly object _gate = new();
    private Action? _pendingStart;
    private Timer? _fallbackTimer;

    public VlcNativeWindowTarget(MediaPlayer player) => _player = player;

    public bool HasWindow { get; private set; }

    public void AttachWindow(IntPtr handle)
    {
        SetHandle(handle);
        HasWindow = true;
        TakePendingStart()?.Invoke();
    }

    public void DetachWindow()
    {
        HasWindow = false;
        SetHandle(IntPtr.Zero);
    }

    /// <summary>Voert <paramref name="start"/> nu uit, of zodra het venster bestaat.</summary>
    public void RunWhenReady(Action start)
    {
        if (HasWindow)
        {
            start();
            return;
        }

        lock (_gate)
        {
            _pendingStart = start;
            _fallbackTimer?.Dispose();
            _fallbackTimer = new Timer(_ => StartWithoutWindow(), null, WindowTimeout, Timeout.InfiniteTimeSpan);
        }
    }

    private void StartWithoutWindow()
    {
        if (TakePendingStart() is not { } start) return;

        CrashLogger.Write("Videovenster niet beschikbaar; afspelen zonder Lumen-videovlak", null);
        start();
    }

    private Action? TakePendingStart()
    {
        lock (_gate)
        {
            _fallbackTimer?.Dispose();
            _fallbackTimer = null;

            var start = _pendingStart;
            _pendingStart = null;
            return start;
        }
    }

    private void SetHandle(IntPtr handle)
    {
        if (OperatingSystem.IsWindows()) _player.Hwnd = handle;
        else if (OperatingSystem.IsLinux()) _player.XWindow = (uint)handle;
        else if (OperatingSystem.IsMacOS()) _player.NsObject = handle;
    }
}
