namespace Lumen.Features.Playback.Subtitles;

/// <summary>Alle regels van een ondertitelbestand, gesorteerd op begintijd.</summary>
public sealed class SubtitleDocument
{
    private readonly SubtitleCue[] _cues;

    public SubtitleDocument(IEnumerable<SubtitleCue> cues)
    {
        _cues = cues.Where(c => c.End > c.Start).OrderBy(c => c.Start).ToArray();
    }

    public int Count => _cues.Length;

    /// <summary>De tekst die op <paramref name="time"/> in beeld hoort, of een lege string.</summary>
    public string TextAt(TimeSpan time)
    {
        var last = LastCueStartingBefore(time);
        if (last < 0) return string.Empty;

        // Regels kunnen overlappen: verzamel alle actieve regels die vóór dit moment begonnen.
        var active = new List<string>();
        for (var i = last; i >= 0 && active.Count < 3; i--)
        {
            if (_cues[i].IsActiveAt(time)) active.Insert(0, _cues[i].Text);
            if (time - _cues[i].Start > TimeSpan.FromSeconds(30)) break;
        }

        return string.Join("\n", active);
    }

    private int LastCueStartingBefore(TimeSpan time)
    {
        int low = 0, high = _cues.Length - 1, result = -1;
        while (low <= high)
        {
            var mid = (low + high) / 2;
            if (_cues[mid].Start <= time)
            {
                result = mid;
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return result;
    }
}
