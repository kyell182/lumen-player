namespace Lumen.Features.Playback;

/// <summary>Een audio- of ondertitelspoor. Id -1 betekent "uitgeschakeld".</summary>
public sealed record TrackInfo(int Id, string Name)
{
    public const int DisabledId = -1;

    public bool IsDisabledTrack => Id == DisabledId;
}
