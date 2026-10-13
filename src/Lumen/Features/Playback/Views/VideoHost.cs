using Avalonia.Controls;
using Avalonia.Platform;
using Lumen.Features.Playback.Video;

namespace Lumen.Features.Playback.Views;

/// <summary>Native venster waarin VLC via de GPU de video tekent.</summary>
public sealed class VideoHost : NativeControlHost
{
    private readonly INativeVideoTarget _target;

    public VideoHost(INativeVideoTarget target) => _target = target;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var handle = base.CreateNativeControlCore(parent);
        _target.AttachWindow(handle.Handle);
        return handle;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        _target.DetachWindow();
        base.DestroyNativeControlCore(control);
    }
}
