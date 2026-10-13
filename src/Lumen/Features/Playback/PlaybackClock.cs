using System.Diagnostics;

namespace Lumen.Features.Playback;

/// <summary>
/// Schat de afspeelpositie tussen de updates van VLC door (die komen maar enkele keren per seconde),
/// zodat ondertitels op het juiste moment verschijnen.
/// </summary>
public sealed class PlaybackClock
{
    private readonly Stopwatch _sinceUpdate = new();
    private TimeSpan _lastPosition;
    private bool _isRunning;
    private double _rate = 1;

    public TimeSpan Now => _isRunning
        ? _lastPosition + TimeSpan.FromTicks((long)(_sinceUpdate.Elapsed.Ticks * _rate))
        : _lastPosition;

    public void Sync(TimeSpan position)
    {
        _lastPosition = position;
        _sinceUpdate.Restart();
    }

    public void SetRunning(bool isRunning)
    {
        if (isRunning == _isRunning) return;
        Sync(Now);
        _isRunning = isRunning;
    }

    public void SetRate(double rate)
    {
        Sync(Now);
        _rate = rate;
    }
}
