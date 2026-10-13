namespace Lumen.Features.Playback.Video;

/// <summary>Leest een frame: BGRA-pixels, breedte, hoogte en het aantal bytes per rij.</summary>
public delegate void FrameReader(byte[] pixels, int width, int height, int stride);

/// <summary>Levert videobeelden aan de UI, zodat Lumen de video zelf kan tekenen.</summary>
public interface IVideoFrameSource
{
    /// <summary>Er is een nieuw frame (of het beeld is leeggemaakt). Komt binnen op een achtergrondthread.</summary>
    event EventHandler? FrameReady;

    /// <summary>Geeft het laatste frame aan <paramref name="reader"/>. False als er geen beeld is.</summary>
    bool ReadLatestFrame(FrameReader reader);
}
