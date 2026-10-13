using System.Text.RegularExpressions;

namespace Lumen.Features.Playback.Subtitles.Parsing;

/// <summary>Leest SubRip (.srt) en WebVTT (.vtt): een tijdregel "a --> b" gevolgd door tekstregels.</summary>
public sealed partial class SrtParser : ISubtitleParser
{
    public IReadOnlyList<SubtitleCue> Parse(string content)
    {
        var cues = new List<SubtitleCue>();
        var lines = content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var timing = TimingLine().Match(lines[i]);
            if (!timing.Success) continue;

            var text = new List<string>();
            while (i + 1 < lines.Length && lines[i + 1].Trim().Length > 0)
            {
                i++;
                var clean = SubtitleText.StripMarkup(lines[i]);
                if (clean.Length > 0) text.Add(clean);
            }

            if (text.Count > 0
                && SubtitleTime.TryParse(timing.Groups["start"].Value, out var start)
                && SubtitleTime.TryParse(timing.Groups["end"].Value, out var end))
            {
                cues.Add(new SubtitleCue(start, end, string.Join("\n", text)));
            }
        }

        return cues;
    }

    [GeneratedRegex(@"(?<start>(\d+:)?\d{1,2}:\d{1,2}[,.]\d{1,3})\s*-->\s*(?<end>(\d+:)?\d{1,2}:\d{1,2}[,.]\d{1,3})")]
    private static partial Regex TimingLine();
}
