using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lumen.Features.Tools;

/// <summary>Korte meldingen bovenaan de video ("Schermafbeelding opgeslagen", "A-B: punt A gezet", …).</summary>
public sealed partial class Notifier : ObservableObject
{
    private static readonly TimeSpan DisplayTime = TimeSpan.FromSeconds(2.5);

    private readonly DispatcherTimer _hideTimer = new() { Interval = DisplayTime };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    private string _message = string.Empty;

    public Notifier() => _hideTimer.Tick += (_, _) =>
    {
        _hideTimer.Stop();
        Message = string.Empty;
    };

    public bool IsVisible => Message.Length > 0;

    public void Show(string message)
    {
        Message = message;
        _hideTimer.Stop();
        _hideTimer.Start();
    }
}
