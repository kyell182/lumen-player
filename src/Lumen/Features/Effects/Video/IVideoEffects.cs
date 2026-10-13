namespace Lumen.Features.Effects.Video;

public interface IVideoEffects
{
    void SetColor(ColorAdjustments adjustments);

    /// <summary>Beeldverhouding als "16:9", of null voor automatisch.</summary>
    void SetAspectRatio(string? ratio);

    /// <summary>Bijsnijden tot "16:9" enz., of null voor niet bijsnijden.</summary>
    void SetCrop(string? ratio);

    /// <summary>0 = passend in het venster, 1 = originele grootte.</summary>
    void SetZoom(float scale);

    void SetDeinterlace(bool enabled);

    /// <summary>"90", "180", "270", "hflip", "vflip" of null. Het bestand wordt hiervoor opnieuw geopend.</summary>
    void SetRotation(string? transform);

    /// <summary>Slaat het huidige beeld op als PNG. True als het gelukt is.</summary>
    bool TakeSnapshot(string filePath);
}
