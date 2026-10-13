using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Lumen.Features.Playback.Video;

namespace Lumen.Features.Playback.Views;

/// <summary>
/// Het videovlak. Eén klik = <see cref="Clicked"/>, dubbelklik = <see cref="DoubleClicked"/>
/// (een enkele klik wacht even, zodat een dubbelklik niet ook als klik telt).
/// </summary>
public partial class PlayerView : UserControl
{
    private static readonly TimeSpan DoubleClickWindow = TimeSpan.FromMilliseconds(250);

    private readonly DispatcherTimer _singleClickTimer;

    public PlayerView()
    {
        InitializeComponent();

        _singleClickTimer = new DispatcherTimer { Interval = DoubleClickWindow };
        _singleClickTimer.Tick += (_, _) =>
        {
            _singleClickTimer.Stop();
            Clicked?.Invoke(this, EventArgs.Empty);
        };

        Tapped += OnTapped;
        DoubleTapped += OnDoubleTapped;
    }

    public event EventHandler? Clicked;
    public event EventHandler? DoubleClicked;

    public void AttachVideo(IVideoFrameSource source) => Video.Attach(source);

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        _singleClickTimer.Stop();
        _singleClickTimer.Start();
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        _singleClickTimer.Stop();
        DoubleClicked?.Invoke(this, EventArgs.Empty);
    }
}
