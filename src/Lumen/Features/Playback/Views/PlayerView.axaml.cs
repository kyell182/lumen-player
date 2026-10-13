using Avalonia.Controls;
using Lumen.Features.Playback.Video;

namespace Lumen.Features.Playback.Views;

/// <summary>Het videovlak: tekent frames zelf, of laat VLC in een native venster tekenen.</summary>
public partial class PlayerView : UserControl
{
    public PlayerView() => InitializeComponent();

    /// <summary>True als de video in een native venster staat (dan kan er niets overheen getekend worden).</summary>
    public bool UsesNativeWindow { get; private set; }

    public void Show(VideoOutput output)
    {
        if (output.Frames is { } frames)
        {
            var surface = new VideoSurface();
            surface.Attach(frames);
            Host.Content = surface;
        }
        else if (output.NativeTarget is { } target)
        {
            Host.Content = new VideoHost(target);
            UsesNativeWindow = true;
        }
    }
}
