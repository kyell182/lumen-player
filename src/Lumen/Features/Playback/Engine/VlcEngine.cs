using LibVLCSharp.Shared;

namespace Lumen.Features.Playback.Engine;

/// <summary>
/// De gedeelde VLC-engine: één LibVLC-instantie en één speler.
/// Alle VLC-adapters (afspelen, effecten, casten, info) werken op deze ene speler.
/// </summary>
public sealed class VlcEngine : IDisposable
{
    private static readonly string[] Options =
    {
        "--no-video-title-show",
        "--no-sub-autodetect-file", // Lumen zoekt ondertitels zelf (SubtitleFinder)
    };

    private readonly VlcLogWriter _log = new();

    public VlcEngine()
    {
        LibVlc = new LibVLC(false, Options);
        _log.Attach(LibVlc);
        Player = new MediaPlayer(LibVlc)
        {
            EnableHardwareDecoding = true,
            EnableKeyInput = false,
            EnableMouseInput = false,
        };
    }

    public LibVLC LibVlc { get; }

    public MediaPlayer Player { get; }

    public void Dispose()
    {
        Player.Dispose();
        LibVlc.Dispose();
    }
}
