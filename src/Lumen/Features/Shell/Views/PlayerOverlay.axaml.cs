using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace Lumen.Features.Shell.Views;

/// <summary>
/// Alles wat over de video ligt: ondertitels en bediening.
/// Eén klik op de video = <see cref="Clicked"/>, dubbelklik = <see cref="DoubleClicked"/>.
/// </summary>
public partial class PlayerOverlay : UserControl
{
    private static readonly TimeSpan DoubleClickWindow = TimeSpan.FromMilliseconds(250);
    private static readonly Cursor HiddenCursor = new(StandardCursorType.None);

    private readonly DispatcherTimer _singleClickTimer;

    public PlayerOverlay()
    {
        InitializeComponent();

        _singleClickTimer = new DispatcherTimer { Interval = DoubleClickWindow };
        _singleClickTimer.Tick += (_, _) =>
        {
            _singleClickTimer.Stop();
            Clicked?.Invoke(this, EventArgs.Empty);
        };

        Root.Tapped += OnTapped;
        Root.DoubleTapped += OnDoubleTapped;
    }

    public event EventHandler? Clicked;
    public event EventHandler? DoubleClicked;

    /// <summary>Bediening zichtbaar: ondertitels omhoog en muisaanwijzer tonen; anders allebei weg.</summary>
    public void SetControlsVisible(bool visible)
    {
        SubtitleLayer.SetRaised(visible);
        Cursor = visible ? Cursor.Default : HiddenCursor;
    }

    // Alleen klikken op de video zelf tellen, niet op de knoppen van de bediening.
    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source != Root) return;
        _singleClickTimer.Stop();
        _singleClickTimer.Start();
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source != Root) return;
        _singleClickTimer.Stop();
        DoubleClicked?.Invoke(this, EventArgs.Empty);
    }
}
