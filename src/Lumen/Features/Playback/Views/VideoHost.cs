using Avalonia.Controls;
using Avalonia.Platform;
using LibVLCSharp.Shared;

namespace Lumen.Features.Playback.Views;

/// <summary>
/// Native venster waarin VLC de video tekent. Koppelt de VLC-speler aan het venster
/// zodra dat bestaat, en ontkoppelt hem weer voordat het venster verdwijnt.
/// </summary>
public sealed class VideoHost : NativeControlHost
{
    private MediaPlayer? _player;
    private IPlatformHandle? _handle;

    public void Attach(MediaPlayer player)
    {
        _player = player;
        Bind();
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        _handle = base.CreateNativeControlCore(parent);
        Bind();
        return _handle;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        Unbind();
        _handle = null;
        base.DestroyNativeControlCore(control);
    }

    private void Bind()
    {
        if (_player is null || _handle is null) return;

        if (OperatingSystem.IsWindows()) _player.Hwnd = _handle.Handle;
        else if (OperatingSystem.IsLinux()) _player.XWindow = (uint)_handle.Handle;
        else if (OperatingSystem.IsMacOS()) _player.NsObject = _handle.Handle;
    }

    private void Unbind()
    {
        if (_player is null) return;

        if (OperatingSystem.IsWindows()) _player.Hwnd = IntPtr.Zero;
        else if (OperatingSystem.IsLinux()) _player.XWindow = 0;
        else if (OperatingSystem.IsMacOS()) _player.NsObject = IntPtr.Zero;
    }
}
