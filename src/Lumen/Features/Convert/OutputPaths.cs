using Lumen.Infrastructure;

namespace Lumen.Features.Convert;

/// <summary>Bedenkt een doelbestand dat nog niet bestaat: "Film (geconverteerd).mp4", "Film (geconverteerd 2).mp4", …</summary>
public static class OutputPaths
{
    public static string DefaultFolder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Lumen");

    public static string For(string source, string? folder, string suffix, string extension)
    {
        var targetFolder = folder
                           ?? (MediaSource.IsStream(source) ? DefaultFolder : Path.GetDirectoryName(Path.GetFullPath(source)))
                           ?? DefaultFolder;
        var name = SafeName(MediaSource.DisplayName(source));

        for (var attempt = 1; ; attempt++)
        {
            var label = attempt == 1 ? suffix : $"{suffix} {attempt}";
            var candidate = Path.Combine(targetFolder, $"{name} ({label}){extension}");
            if (!File.Exists(candidate)) return candidate;
        }
    }

    private static string SafeName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var clean = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
        return clean.Length == 0 ? "media" : clean;
    }
}
