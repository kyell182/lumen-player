using System.Text.RegularExpressions;

namespace Lumen.Features.Playback.Tracks;

/// <summary>
/// Maakt leesbare spoornamen.
/// "English-SDH-SRT - [English]" wordt "Engels (SDH)", "Film.nl.forced.srt" wordt "Nederlands (geforceerd, bestand)".
/// </summary>
public static partial class TrackLabel
{
    private static readonly char[] Separators = { '.', '_', '-', ' ', '(', ')', '[', ']', ',' };

    private static readonly Dictionary<string, string> FlagNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["forced"] = "geforceerd",
        ["foreign"] = "geforceerd",
        ["sdh"] = "SDH",
        ["cc"] = "SDH",
        ["commentary"] = "commentaar",
    };

    /// <summary>Naam voor een spoor zoals VLC het beschrijft.</summary>
    public static string ForEngineTrack(string engineName, int id)
    {
        var match = EngineNamePattern().Match(engineName);
        var title = match.Success ? match.Groups["title"].Value : engineName;
        var language = LanguageNames.ToDutch(match.Success ? match.Groups["lang"].Value : null)
                       ?? LanguageNames.FirstIn(Tokens(title));

        if (language is null)
        {
            return title.StartsWith("Track", StringComparison.OrdinalIgnoreCase) ? $"Spoor {id}" : title;
        }

        return WithFlags(language, Flags(title));
    }

    /// <summary>
    /// Naam voor een los ondertitelbestand. Alleen het deel ná de videonaam telt
    /// ("Film.nl.srt" → "nl"), zodat woorden uit de filmtitel niet als taal gelezen worden.
    /// </summary>
    public static string ForFile(string path, string videoName)
    {
        var tokens = FileSuffixTokens(path, videoName);
        var language = LanguageNames.FirstIn(tokens) ?? "Ondertitels";
        return WithFlags(language, Flags(tokens).Append("bestand"));
    }

    /// <summary>De woorden in de bestandsnaam die niet uit de videonaam komen.</summary>
    public static IReadOnlyList<string> FileSuffixTokens(string path, string videoName)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var suffix = name.StartsWith(videoName, StringComparison.OrdinalIgnoreCase) ? name[videoName.Length..] : name;
        return Tokens(suffix).ToList();
    }

    public static IEnumerable<string> Tokens(string text) =>
        text.Split(Separators, StringSplitOptions.RemoveEmptyEntries);

    private static IEnumerable<string> Flags(string text) => Flags(Tokens(text));

    private static IEnumerable<string> Flags(IEnumerable<string> tokens) =>
        tokens.Select(t => FlagNames.GetValueOrDefault(t)).OfType<string>().Distinct();

    private static string WithFlags(string language, IEnumerable<string> flags)
    {
        var list = flags.ToList();
        return list.Count == 0 ? language : $"{language} ({string.Join(", ", list)})";
    }

    [GeneratedRegex(@"^(?<title>.*?)\s*-\s*\[(?<lang>[^\]]+)\]\s*$")]
    private static partial Regex EngineNamePattern();
}
