using Lumen.Features.History;
using Lumen.Infrastructure;

namespace Lumen.Features.Home;

/// <summary>Een kaart onder "Verder kijken".</summary>
public sealed class RecentItemViewModel
{
    public RecentItemViewModel(RecentItem item)
    {
        Path = item.Path;
        Title = System.IO.Path.GetFileNameWithoutExtension(item.Path);
        IsAudio = MediaFileTypes.KindOf(item.Path) == MediaKind.Audio;
        ProgressPercent = item.Progress * 100;
        HasProgress = item.Progress > 0.01 && !item.IsFinished;
        Detail = DescribeProgress(item);
    }

    public string Path { get; }

    public string Title { get; }

    public bool IsAudio { get; }

    public bool IsVideo => !IsAudio;

    public double ProgressPercent { get; }

    public bool HasProgress { get; }

    public string Detail { get; }

    private static string DescribeProgress(RecentItem item)
    {
        if (item.IsFinished) return "Bekeken";
        if (item.DurationSeconds <= 0) return "Recent geopend";

        var remaining = TimeSpan.FromSeconds(item.DurationSeconds - item.PositionSeconds);
        return remaining.TotalMinutes >= 1
            ? $"Nog {(int)Math.Ceiling(remaining.TotalMinutes)} min"
            : "Bijna klaar";
    }
}
