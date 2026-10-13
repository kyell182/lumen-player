using Avalonia.Threading;

namespace Lumen.Features.Shell;

/// <summary>Roept een actie aan wanneer de gebruiker een tijdje niets doet.</summary>
public sealed class ControlsAutoHider
{
    private readonly DispatcherTimer _timer;

    public ControlsAutoHider(TimeSpan delay, Action onIdle)
    {
        _timer = new DispatcherTimer { Interval = delay };
        _timer.Tick += (_, _) =>
        {
            _timer.Stop();
            onIdle();
        };
    }

    public void Restart()
    {
        _timer.Stop();
        _timer.Start();
    }

    public void Stop() => _timer.Stop();
}
