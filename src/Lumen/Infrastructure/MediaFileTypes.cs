namespace Lumen.Infrastructure;

public enum MediaKind
{
    Unknown,
    Video,
    Audio,
}

/// <summary>Weet welke bestanden Lumen kan afspelen en zet mappen om naar bestandslijsten.</summary>
public static class MediaFileTypes
{
    public static readonly IReadOnlyList<string> VideoExtensions = new[]
    {
        "mp4", "mkv", "avi", "mov", "wmv", "webm", "m4v", "flv", "mpg", "mpeg", "ts", "m2ts", "3gp", "ogv",
    };

    public static readonly IReadOnlyList<string> AudioExtensions = new[]
    {
        "mp3", "flac", "wav", "aac", "m4a", "ogg", "opus", "wma", "aiff",
    };

    public static readonly IReadOnlyList<string> SubtitleExtensions = new[]
    {
        "srt", "ass", "ssa", "vtt", "sub",
    };

    public static MediaKind KindOf(string path)
    {
        var extension = ExtensionOf(path);
        if (VideoExtensions.Contains(extension)) return MediaKind.Video;
        if (AudioExtensions.Contains(extension)) return MediaKind.Audio;
        return MediaKind.Unknown;
    }

    public static bool IsPlayable(string path) => KindOf(path) != MediaKind.Unknown;

    public static bool IsSubtitle(string path) => SubtitleExtensions.Contains(ExtensionOf(path));

    /// <summary>Filtert afspeelbare bestanden en vervangt mappen door hun inhoud (natuurlijk gesorteerd).</summary>
    public static IReadOnlyList<string> Expand(IEnumerable<string> paths)
    {
        var result = new List<string>();
        foreach (var path in paths)
        {
            if (Directory.Exists(path))
            {
                result.AddRange(PlayableFilesIn(path));
            }
            else if (File.Exists(path) && IsPlayable(path))
            {
                result.Add(path);
            }
        }

        return result;
    }

    private static IEnumerable<string> PlayableFilesIn(string folder)
    {
        try
        {
            return Directory.EnumerateFiles(folder)
                .Where(IsPlayable)
                .OrderBy(Path.GetFileName, NaturalStringComparer.Instance)
                .ToList();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }
    }

    private static string ExtensionOf(string path) =>
        Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
}
