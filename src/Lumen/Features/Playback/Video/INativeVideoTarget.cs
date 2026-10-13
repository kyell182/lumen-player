namespace Lumen.Features.Playback.Video;

/// <summary>Een speler die rechtstreeks in een native venster kan tekenen.</summary>
public interface INativeVideoTarget
{
    /// <summary>Het venster bestaat: vanaf nu tekent de speler hierin.</summary>
    void AttachWindow(IntPtr handle);

    /// <summary>Het venster verdwijnt.</summary>
    void DetachWindow();
}

/// <summary>Hoe de video in beeld komt: als frames, of in een native venster.</summary>
public sealed record VideoOutput(IVideoFrameSource? Frames, INativeVideoTarget? NativeTarget);
