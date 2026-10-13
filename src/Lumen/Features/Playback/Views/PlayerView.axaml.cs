using Avalonia.Controls;

namespace Lumen.Features.Playback.Views;

/// <summary>Het videovlak. VLC tekent hier rechtstreeks in een native venster.</summary>
public partial class PlayerView : UserControl
{
    public PlayerView() => InitializeComponent();

    public void AttachVideo(IVlcVideoSource source) => VideoSurface.Attach(source.Surface);
}
