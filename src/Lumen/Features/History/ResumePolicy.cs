namespace Lumen.Features.History;

/// <summary>Beslist of een bestand verderspeelt waar je stopte, of opnieuw begint.</summary>
public static class ResumePolicy
{
    private static readonly TimeSpan MinimumDuration = TimeSpan.FromMinutes(5);  // korte nummers niet hervatten
    private static readonly TimeSpan MinimumWatched = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan MinimumRemaining = TimeSpan.FromSeconds(15);

    public static TimeSpan StartPositionFor(RecentItem? item)
    {
        if (item is null || item.IsFinished) return TimeSpan.Zero;

        var position = TimeSpan.FromSeconds(item.PositionSeconds);
        var duration = TimeSpan.FromSeconds(item.DurationSeconds);

        bool worthResuming = duration >= MinimumDuration
            && position >= MinimumWatched
            && duration - position >= MinimumRemaining;

        return worthResuming ? position : TimeSpan.Zero;
    }
}
