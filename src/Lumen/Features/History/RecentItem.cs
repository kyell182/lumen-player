using System.Text.Json.Serialization;

namespace Lumen.Features.History;

/// <summary>Eén recent afgespeeld bestand met de positie waar de gebruiker stopte.</summary>
public sealed record RecentItem(string Path, double PositionSeconds, double DurationSeconds, DateTimeOffset LastPlayed)
{
    private const double FinishedRatio = 0.95;

    [JsonIgnore]
    public bool IsFinished => DurationSeconds > 0 && PositionSeconds >= DurationSeconds * FinishedRatio;

    [JsonIgnore]
    public double Progress => DurationSeconds > 0 ? Math.Clamp(PositionSeconds / DurationSeconds, 0, 1) : 0;
}
