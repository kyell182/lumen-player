using Lumen.Features.Playback.Tracks;

namespace Lumen.Features.Playback.Subtitles;

/// <summary>Rangschikt ondertitels op voorkeur: Nederlands, Engels, rest. Geforceerde ondertitels achteraan.</summary>
public static class SubtitleLanguage
{
    private static readonly HashSet<string> ForcedTokens = new(StringComparer.OrdinalIgnoreCase) { "forced", "foreign" };

    /// <summary>Lager = beter.</summary>
    public static int PreferenceRank(string path, string videoName)
    {
        var tokens = TrackLabel.FileSuffixTokens(path, videoName);
        var penalty = tokens.Any(ForcedTokens.Contains) ? 10 : 0;

        return LanguageNames.FirstIn(tokens) switch
        {
            LanguageNames.Dutch => penalty,
            LanguageNames.English => penalty + 1,
            _ => penalty + 2,
        };
    }
}
