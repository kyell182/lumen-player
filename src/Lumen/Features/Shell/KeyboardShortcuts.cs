using Avalonia.Input;

namespace Lumen.Features.Shell;

/// <summary>Vertaalt toetsen naar acties. Geeft true terug als de toets is afgehandeld.</summary>
public sealed class KeyboardShortcuts
{
    private const double SeekStep = 10;
    private const double VolumeStep = 5;
    private const KeyModifiers BlockingModifiers = KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Meta;

    private readonly MainViewModel _vm;

    public KeyboardShortcuts(MainViewModel vm) => _vm = vm;

    public bool TryHandle(Key key, KeyModifiers modifiers)
    {
        if (modifiers == KeyModifiers.Control && key == Key.O)
        {
            _vm.OpenFilesCommand.Execute(null);
            return true;
        }

        if ((modifiers & BlockingModifiers) != 0 || _vm.IsHomeVisible)
        {
            return false;
        }

        if (!TryHandlePlaybackKey(key))
        {
            return false;
        }

        _vm.ReportUserActivity();
        return true;
    }

    private bool TryHandlePlaybackKey(Key key)
    {
        var player = _vm.Player;
        switch (key)
        {
            case Key.Space:
            case Key.K:
                player.TogglePlayPause();
                return true;
            case Key.Left:
            case Key.J:
                player.SeekBy(-SeekStep);
                return true;
            case Key.Right:
            case Key.L:
                player.SeekBy(SeekStep);
                return true;
            case Key.Up:
                player.ChangeVolume(VolumeStep);
                return true;
            case Key.Down:
                player.ChangeVolume(-VolumeStep);
                return true;
            case Key.M:
                player.ToggleMuteCommand.Execute(null);
                return true;
            case Key.F:
            case Key.F11:
                _vm.ToggleFullScreenCommand.Execute(null);
                return true;
            case Key.Escape when _vm.IsFullScreen:
                _vm.ExitFullScreen();
                return true;
            case Key.N:
                _vm.NextCommand.Execute(null);
                return true;
            case Key.P:
                _vm.PreviousCommand.Execute(null);
                return true;
            case Key.Home:
                player.SeekTo(0);
                return true;
            case Key.OemPlus:
            case Key.Add:
                player.StepRate(+1);
                return true;
            case Key.OemMinus:
            case Key.Subtract:
                player.StepRate(-1);
                return true;
            default:
                return false;
        }
    }
}
