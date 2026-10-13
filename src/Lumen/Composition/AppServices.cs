using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Lumen.Features.History;
using Lumen.Features.Home;
using Lumen.Features.Playback;
using Lumen.Features.Playback.Video;
using Lumen.Features.Playlist;
using Lumen.Features.Settings;
using Lumen.Features.Settings.Skins;
using Lumen.Features.Shell;
using Lumen.Features.Updates;
using Lumen.Features.Updates.Installers;
using Lumen.Infrastructure;

namespace Lumen.Composition;

/// <summary>Compositieroot: hier worden alle services één keer aangemaakt en aan elkaar gekoppeld.</summary>
public sealed class AppServices : IDisposable
{
    private readonly VlcMediaPlayerService _mediaPlayer;
    private readonly HttpClient _http = CreateHttpClient();

    public AppServices(Func<TopLevel?> topLevel)
    {
        Settings = new JsonSettingsStore(new JsonFileStore<AppSettings>(AppPaths.DataFile("settings.json")));
        History = new JsonHistoryStore(new JsonFileStore<List<RecentItem>>(AppPaths.DataFile("history.json")));
        Theme = new ThemeService(Settings);
        Skins = new SkinService(Settings);
        FilePicker = new FilePickerService(topLevel);
        _mediaPlayer = new VlcMediaPlayerService(VideoOutputModes.ForCurrentPlatform());
        Updates = new UpdateService(
            new GitHubReleaseSource(_http),
            new UpdateDownloader(_http),
            UpdateInstallerFactory.ForCurrentPlatform());
    }

    public ISettingsStore Settings { get; }

    public IHistoryStore History { get; }

    public ThemeService Theme { get; }

    public SkinService Skins { get; }

    public IFilePickerService FilePicker { get; }

    public UpdateService Updates { get; }

    public IMediaPlayerService MediaPlayer => _mediaPlayer;

    public VideoOutput VideoOutput => _mediaPlayer.Output;

    public MainViewModel CreateMainViewModel() => new(
        new PlayerViewModel(MediaPlayer, Settings.Current.Volume),
        new PlaylistViewModel(FilePicker),
        new HomeViewModel(History),
        new UpdateViewModel(Updates, ShutdownApp),
        History,
        Settings,
        Theme,
        Skins,
        FilePicker);

    public void Dispose()
    {
        _mediaPlayer.Dispose();
        _http.Dispose();
    }

    private static void ShutdownApp() =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();

    private static HttpClient CreateHttpClient()
    {
        var http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("Lumen-Updater");
        return http;
    }
}
