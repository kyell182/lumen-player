using Avalonia.Controls;
using Lumen.Features.History;
using Lumen.Features.Home;
using Lumen.Features.Playback;
using Lumen.Features.Playlist;
using Lumen.Features.Settings;
using Lumen.Features.Shell;
using Lumen.Infrastructure;

namespace Lumen.Composition;

/// <summary>Compositieroot: hier worden alle services één keer aangemaakt en aan elkaar gekoppeld.</summary>
public sealed class AppServices : IDisposable
{
    private readonly VlcMediaPlayerService _mediaPlayer;

    public AppServices(Func<TopLevel?> topLevel)
    {
        Settings = new JsonSettingsStore(new JsonFileStore<AppSettings>(AppPaths.DataFile("settings.json")));
        History = new JsonHistoryStore(new JsonFileStore<List<RecentItem>>(AppPaths.DataFile("history.json")));
        Theme = new ThemeService(Settings);
        FilePicker = new FilePickerService(topLevel);
        _mediaPlayer = new VlcMediaPlayerService();
    }

    public ISettingsStore Settings { get; }

    public IHistoryStore History { get; }

    public ThemeService Theme { get; }

    public IFilePickerService FilePicker { get; }

    public IMediaPlayerService MediaPlayer => _mediaPlayer;

    public IVlcVideoSource VideoSource => _mediaPlayer;

    public MainViewModel CreateMainViewModel() => new(
        new PlayerViewModel(MediaPlayer, Settings.Current.Volume),
        new PlaylistViewModel(FilePicker),
        new HomeViewModel(History),
        History,
        Settings,
        Theme,
        FilePicker);

    public void Dispose() => _mediaPlayer.Dispose();
}
