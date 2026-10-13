using Avalonia.Threading;

namespace Lumen.Features.Tools;

/// <summary>Pauzeert na een gekozen tijd, of aan het einde van het huidige bestand.</summary>
public sealed class SleepTimer
{
    public static readonly IReadOnlyList<int> MinuteChoices = new[] { 15, 30, 45, 60, 90, 120 };

    private readonly DispatcherTimer _timer = new();
    private DateTime _deadline;

    public SleepTimer() => _timer.Tick += (_, _) => Fire();

    public event EventHandler? Elapsed;

    public bool IsActive => _timer.IsEnabled || StopAtEndOfItem;

    public bool StopAtEndOfItem { get; private set; }

    public string Description => StopAtEndOfItem
        ? "Stopt na dit bestand"
        : _timer.IsEnabled ? $"Stopt om {_deadline:HH:mm}" : "Uit";

    public void StartMinutes(int minutes)
    {
        Cancel();
        _deadline = DateTime.Now.AddMinutes(minutes);
        _timer.Interval = TimeSpan.FromMinutes(minutes);
        _timer.Start();
    }

    public void StartAtEndOfItem()
    {
        Cancel();
        StopAtEndOfItem = true;
    }

    public void Cancel()
    {
        _timer.Stop();
        StopAtEndOfItem = false;
    }

    /// <summary>Aanroepen wanneer een bestand afgelopen is. True = niet verder spelen.</summary>
    public bool ConsumeEndOfItem()
    {
        if (!StopAtEndOfItem) return false;
        StopAtEndOfItem = false;
        Elapsed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private void Fire()
    {
        _timer.Stop();
        Elapsed?.Invoke(this, EventArgs.Empty);
    }
}
