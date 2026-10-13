using LibVLCSharp.Shared;

namespace Lumen.Features.Playback;

/// <summary>Geeft de video-weergave toegang tot de VLC-speler, zonder de rest van de app aan VLC te koppelen.</summary>
public interface IVlcVideoSource
{
    MediaPlayer Surface { get; }
}
