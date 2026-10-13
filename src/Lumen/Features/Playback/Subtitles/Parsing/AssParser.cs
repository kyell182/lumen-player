namespace Lumen.Features.Playback.Subtitles.Parsing;

/// <summary>
/// Leest Advanced SubStation Alpha (.ass/.ssa). Alleen de tekst wordt getoond;
/// stijlen, posities en effecten worden genegeerd.
/// </summary>
public sealed class AssParser : ISubtitleParser
{
    private static readonly string[] DefaultFormat = { "layer", "start", "end", "style", "name", "marginl", "marginr", "marginv", "effect", "text" };

    public IReadOnlyList<SubtitleCue> Parse(string content)
    {
        var cues = new List<SubtitleCue>();
        var format = DefaultFormat;
        var inEvents = false;

        foreach (var rawLine in content.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.StartsWith('['))
            {
                inEvents = line.Equals("[Events]", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!inEvents) continue;

            if (TryReadField(line, "Format:", out var formatValue))
            {
                format = formatValue.Split(',').Select(f => f.Trim().ToLowerInvariant()).ToArray();
            }
            else if (TryReadField(line, "Dialogue:", out var dialogue) && TryParseDialogue(dialogue, format, out var cue))
            {
                cues.Add(cue);
            }
        }

        return cues;
    }

    private static bool TryParseDialogue(string value, string[] format, out SubtitleCue cue)
    {
        cue = null!;
        // De tekst is altijd het laatste veld en mag zelf komma's bevatten.
        var fields = value.Split(',', format.Length);
        if (fields.Length < format.Length) return false;

        var start = Array.IndexOf(format, "start");
        var end = Array.IndexOf(format, "end");
        var text = Array.IndexOf(format, "text");
        if (start < 0 || end < 0 || text < 0) return false;

        if (!SubtitleTime.TryParse(fields[start], out var startTime) || !SubtitleTime.TryParse(fields[end], out var endTime))
        {
            return false;
        }

        var clean = SubtitleText.StripMarkup(fields[text]);
        if (clean.Length == 0) return false;

        cue = new SubtitleCue(startTime, endTime, clean);
        return true;
    }

    private static bool TryReadField(string line, string key, out string value)
    {
        value = line.StartsWith(key, StringComparison.OrdinalIgnoreCase) ? line[key.Length..].Trim() : string.Empty;
        return value.Length > 0;
    }
}
