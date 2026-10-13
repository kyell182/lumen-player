namespace Lumen.Features.Playback.Subtitles;

/// <summary>Herkent de taal in een ondertitelnaam ("Film.nl.srt", "2_Dutch.srt") en rangschikt: Nederlands, Engels, rest.</summary>
public static class SubtitleLanguage
{
    private static readonly char[] Separators = { '.', '_', '-', ' ', '(', ')', '[', ']' };

    private static readonly HashSet<string> DutchTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "nl", "nld", "dut", "dutch", "nederlands", "vlaams", "flemish",
    };

    private static readonly HashSet<string> EnglishTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "en", "eng", "english", "engels",
    };

    private static readonly HashSet<string> ForcedTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "forced", "foreign",
    };

    /// <summary>Lager = beter. Geforceerde ondertitels (alleen anderstalige stukjes) komen achteraan.</summary>
    public static int PreferenceRank(string subtitlePath)
    {
        var tokens = Path.GetFileNameWithoutExtension(subtitlePath)
            .Split(Separators, StringSplitOptions.RemoveEmptyEntries);

        var penalty = tokens.Any(ForcedTokens.Contains) ? 10 : 0;
        if (tokens.Any(DutchTokens.Contains)) return penalty;
        if (tokens.Any(EnglishTokens.Contains)) return penalty + 1;
        return penalty + 2;
    }
}
