using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Lumen.Features.Cast.Views;
using Lumen.Features.Playback.Views;

namespace Lumen.Features.Shell.Views;

/// <summary>Bedieningsbalk die over de video ligt.</summary>
public partial class ControlBar : UserControl
{
    public ControlBar()
    {
        InitializeComponent();
        WireSeekSlider();
        WireMenus();
        PointerEntered += (_, _) => ViewModel?.SetPointerOverControls(true);
        PointerExited += (_, _) => ViewModel?.SetPointerOverControls(false);
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    // Tijdens het slepen pauzeren we de positie-updates en zoeken pas bij loslaten.
    private void WireSeekSlider()
    {
        SeekSlider.AddHandler(PointerPressedEvent, (_, _) => ViewModel?.Player.BeginUserSeek(),
            RoutingStrategies.Tunnel, handledEventsToo: true);
        SeekSlider.AddHandler(PointerReleasedEvent, (_, _) => ViewModel?.Player.EndUserSeek(),
            RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    private void WireMenus()
    {
        SpeedButton.Click += (_, _) =>
        {
            if (ViewModel is { } vm) PlaybackMenus.ShowSpeed(SpeedButton, vm.Player);
        };
        AudioButton.Click += (_, _) =>
        {
            if (ViewModel is { } vm) PlaybackMenus.ShowAudioTracks(AudioButton, vm.Player);
        };
        CastButton.Click += (_, _) =>
        {
            if (ViewModel is { } vm) CastMenu.Show(CastButton, vm.Cast);
        };
        MoreButton.Click += (_, _) =>
        {
            if (ViewModel is { } vm) MoreMenu.Show(MoreButton, vm);
        };
        SubtitleButton.Click += (_, _) =>
        {
            if (ViewModel is { } vm)
            {
                PlaybackMenus.ShowSubtitles(SubtitleButton, vm.Player, () => vm.LoadSubtitleFileCommand.Execute(null));
            }
        };
    }
}
