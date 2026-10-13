namespace Lumen.Features.Playback.Subtitles;

public enum SubtitleSource
{
    Off,
    Embedded,
    File,
}

/// <summary>Een keuze in het ondertitelmenu: uit, een spoor uit de video, of een los bestand.</summary>
public sealed record SubtitleOption(SubtitleSource Source, string Label, bool IsSelected, int EmbeddedId = -1, ExternalSubtitle? File = null);

/// <summary>Een los ondertitelbestand dat Lumen zelf tekent.</summary>
public sealed record ExternalSubtitle(string Path, string Label, SubtitleDocument Document);
