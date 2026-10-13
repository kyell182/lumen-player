using Lumen.Infrastructure;

namespace Lumen.Features.Playback.Subtitles;

/// <summary>
/// Zoekt ondertitelbestanden bij een video:
/// in dezelfde map, in submappen zoals "Subs" of "Subtitles", en in "Subs/&lt;videonaam&gt;/".
/// Resultaat staat in volgorde van voorkeur (beste eerst).
/// </summary>
public static class SubtitleFinder
{
    private static readonly string[] SubtitleFolderNames = { "subs", "sub", "subtitles", "subtitle", "ondertitels" };

    private enum Match
    {
        ExactName = 0,
        NamePrefix = 1,
        OwnFolder = 1,
        OnlyVideoInFolder = 2,
        None = 99,
    }

    public static IReadOnlyList<string> FindFor(string videoPath)
    {
        var folder = Path.GetDirectoryName(videoPath);
        if (folder is null || !Directory.Exists(folder)) return Array.Empty<string>();

        var videoName = Path.GetFileNameWithoutExtension(videoPath);
        var isOnlyVideo = CountVideoFiles(folder) == 1;

        return CandidateFiles(folder, videoName)
            .Select(c => (c.Path, Match: Classify(c.Path, c.InOwnFolder, videoName, isOnlyVideo)))
            .Where(c => c.Match != Match.None)
            .OrderBy(c => (int)c.Match)
            .ThenBy(c => SubtitleLanguage.PreferenceRank(c.Path))
            .ThenBy(c => Path.GetFileName(c.Path), NaturalStringComparer.Instance)
            .Select(c => c.Path)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static Match Classify(string subtitlePath, bool inOwnFolder, string videoName, bool isOnlyVideo)
    {
        var name = Path.GetFileNameWithoutExtension(subtitlePath);

        if (name.Equals(videoName, StringComparison.OrdinalIgnoreCase)) return Match.ExactName;
        if (StartsWithVideoName(name, videoName)) return Match.NamePrefix;
        if (inOwnFolder) return Match.OwnFolder;
        return isOnlyVideo ? Match.OnlyVideoInFolder : Match.None;
    }

    // "Film.nl", "Film_English", "Film - Dutch" horen bij "Film".
    private static bool StartsWithVideoName(string name, string videoName) =>
        name.Length > videoName.Length
        && name.StartsWith(videoName, StringComparison.OrdinalIgnoreCase)
        && !char.IsLetterOrDigit(name[videoName.Length]);

    private static IEnumerable<(string Path, bool InOwnFolder)> CandidateFiles(string folder, string videoName)
    {
        foreach (var file in SubtitlesIn(folder)) yield return (file, false);

        foreach (var subFolder in SubtitleFolders(folder))
        {
            foreach (var file in SubtitlesIn(subFolder)) yield return (file, false);

            // Veelgebruikte indeling: Subs/<videonaam>/2_English.srt
            var ownFolder = Path.Combine(subFolder, videoName);
            foreach (var file in SubtitlesIn(ownFolder)) yield return (file, true);
        }
    }

    private static IEnumerable<string> SubtitleFolders(string folder) =>
        SafeEnumerate(() => Directory.EnumerateDirectories(folder))
            .Where(d => SubtitleFolderNames.Contains(Path.GetFileName(d), StringComparer.OrdinalIgnoreCase));

    private static IEnumerable<string> SubtitlesIn(string folder) =>
        Directory.Exists(folder)
            ? SafeEnumerate(() => Directory.EnumerateFiles(folder)).Where(MediaFileTypes.IsSubtitle)
            : Enumerable.Empty<string>();

    private static int CountVideoFiles(string folder) =>
        SafeEnumerate(() => Directory.EnumerateFiles(folder)).Count(f => MediaFileTypes.KindOf(f) == MediaKind.Video);

    private static IReadOnlyList<string> SafeEnumerate(Func<IEnumerable<string>> enumerate)
    {
        try
        {
            return enumerate().ToList();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Array.Empty<string>();
        }
    }
}
