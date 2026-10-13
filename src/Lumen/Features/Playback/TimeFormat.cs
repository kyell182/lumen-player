using System.Globalization;

namespace Lumen.Features.Playback;

public static class TimeFormat
{
    public static string Format(double seconds) => Format(TimeSpan.FromSeconds(Math.Max(0, seconds)));

    public static string Format(TimeSpan time) => time.TotalHours >= 1
        ? $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}"
        : $"{time.Minutes}:{time.Seconds:00}";

    public static string FormatRate(float rate) =>
        rate.ToString("0.##", CultureInfo.InvariantCulture) + "×";
}
