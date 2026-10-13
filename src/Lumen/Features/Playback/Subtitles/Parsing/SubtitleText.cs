using System.Net;
using System.Text.RegularExpressions;

namespace Lumen.Features.Playback.Subtitles.Parsing;

/// <summary>Haalt opmaak uit ondertiteltekst: HTML-tags (&lt;i&gt;), ASS-codes ({\an8}) en regeleinden (\N).</summary>
public static partial class SubtitleText
{
    public static string StripMarkup(string text)
    {
        var withoutTags = AssOverrideBlock().Replace(HtmlTag().Replace(text, string.Empty), string.Empty);
        var withBreaks = withoutTags.Replace("\\N", "\n").Replace("\\n", "\n").Replace("\\h", " ");
        var decoded = WebUtility.HtmlDecode(withBreaks);

        return string.Join("\n", decoded.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0));
    }

    [GeneratedRegex(@"<[^>]*>")]
    private static partial Regex HtmlTag();

    [GeneratedRegex(@"\{[^}]*\}")]
    private static partial Regex AssOverrideBlock();
}
