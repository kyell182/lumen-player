namespace Lumen.Infrastructure;

/// <summary>
/// Een af te spelen bron is ofwel een lokaal bestand, ofwel een netwerkadres (http, rtsp, …).
/// Deze helper behandelt beide als gewone tekst en weet het verschil.
/// </summary>
public static class MediaSource
{
    private static readonly HashSet<string> StreamSchemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "http", "https", "rtsp", "rtmp", "rtp", "udp", "mms", "mmsh", "ftp", "smb", "srt",
    };

    public static bool IsStream(string source) =>
        Uri.TryCreate(source, UriKind.Absolute, out var uri) && StreamSchemes.Contains(uri.Scheme);

    /// <summary>Een leesbare naam: bestandsnaam zonder extensie, of de laatste deel van het adres.</summary>
    public static string DisplayName(string source)
    {
        if (!IsStream(source)) return Path.GetFileNameWithoutExtension(source);

        var uri = new Uri(source);
        var lastSegment = Uri.UnescapeDataString(uri.Segments.LastOrDefault()?.Trim('/') ?? string.Empty);
        var name = Path.GetFileNameWithoutExtension(lastSegment);
        return string.IsNullOrWhiteSpace(name) ? uri.Host : name;
    }

    /// <summary>Het adres zoals VLC het verwacht.</summary>
    public static Uri ToUri(string source) =>
        IsStream(source) ? new Uri(source) : new Uri(Path.GetFullPath(source));

    /// <summary>Bestaat de bron nog? Netwerkadressen worden altijd als beschikbaar beschouwd.</summary>
    public static bool IsAvailable(string source) => IsStream(source) || File.Exists(source);
}
