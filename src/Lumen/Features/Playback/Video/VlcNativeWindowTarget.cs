using LibVLCSharp.Shared;

namespace Lumen.Features.Playback.Video;

/// <summary>
/// Koppelt VLC aan een native venster van Lumen. Wordt er afgespeeld voordat het venster bestaat,
/// dan wacht het afspelen tot het venster er is — anders opent VLC een eigen venster.
/// </summary>
public sealed class VlcNativeWindowTarget : INativeVideoTarget
{
    private readonly MediaPlayer _player;
    private Action? _pendingStart;

    public VlcNativeWindowTarget(MediaPlayer player) => _player = player;

    public bool HasWindow { get; private set; }

    public void AttachWindow(IntPtr handle)
    {
        SetHandle(handle);
        HasWindow = true;

        var start = _pendingStart;
        _pendingStart = null;
        start?.Invoke();
    }

    public void DetachWindow()
    {
        HasWindow = false;
        SetHandle(IntPtr.Zero);
    }

    /// <summary>Voert <paramref name="start"/> nu uit, of zodra het venster bestaat.</summary>
    public void RunWhenReady(Action start)
    {
        if (HasWindow) start();
        else _pendingStart = start;
    }

    private void SetHandle(IntPtr handle)
    {
        if (OperatingSystem.IsWindows()) _player.Hwnd = handle;
        else if (OperatingSystem.IsLinux()) _player.XWindow = (uint)handle;
        else if (OperatingSystem.IsMacOS()) _player.NsObject = handle;
    }
}
