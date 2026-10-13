using System.Globalization;

namespace Lumen.Features.Playback.Subtitles.Parsing;

/// <summary>Leest tijden als "01:02:03,456", "01:02:03.456", "02:03.456" of "1:02:03.45".</summary>
public static class SubtitleTime
{
    public static bool TryParse(string text, out TimeSpan time)
    {
        time = TimeSpan.Zero;
        var parts = text.Trim().Replace(',', '.').Split(':');
        if (parts.Length is < 2 or > 3) return false;

        var hours = parts.Length == 3 ? parts[0] : "0";
        var minutes = parts[^2];
        var seconds = parts[^1];

        if (!int.TryParse(hours, NumberStyles.Integer, CultureInfo.InvariantCulture, out var h)
            || !int.TryParse(minutes, NumberStyles.Integer, CultureInfo.InvariantCulture, out var m)
            || !double.TryParse(seconds, NumberStyles.Float, CultureInfo.InvariantCulture, out var s))
        {
            return false;
        }

        time = TimeSpan.FromHours(h) + TimeSpan.FromMinutes(m) + TimeSpan.FromSeconds(s);
        return true;
    }
}
