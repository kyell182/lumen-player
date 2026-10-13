namespace Lumen.Features.Playback;

public enum AbRepeatState
{
    Off,
    StartSet,
    Looping,
}

/// <summary>
/// Herhaal een stuk tussen punt A en punt B. Eerste keer markeren zet A, tweede keer B (en start de lus),
/// derde keer zet de lus uit.
/// </summary>
public sealed class AbRepeat
{
    private static readonly TimeSpan MinimumLength = TimeSpan.FromSeconds(0.5);

    public AbRepeatState State { get; private set; }

    public TimeSpan Start { get; private set; }

    public TimeSpan End { get; private set; }

    public AbRepeatState Mark(TimeSpan position)
    {
        switch (State)
        {
            case AbRepeatState.Off:
                Start = position;
                State = AbRepeatState.StartSet;
                break;
            case AbRepeatState.StartSet when position - Start >= MinimumLength:
                End = position;
                State = AbRepeatState.Looping;
                break;
            case AbRepeatState.StartSet:
                break; // B moet na A liggen
            default:
                Clear();
                break;
        }

        return State;
    }

    public void Clear() => State = AbRepeatState.Off;

    /// <summary>Waar naartoe terugspringen als B bereikt is, of null.</summary>
    public TimeSpan? JumpTarget(TimeSpan position) =>
        State == AbRepeatState.Looping && position >= End ? Start : null;
}
