namespace Lumen.Features.Playback.Subtitles;

/// <summary>Eén ondertitelregel met het moment waarop ze verschijnt en verdwijnt.</summary>
public sealed record SubtitleCue(TimeSpan Start, TimeSpan End, string Text)
{
    public bool IsActiveAt(TimeSpan time) => time >= Start && time < End;
}
