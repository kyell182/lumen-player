using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Lumen.Features.Cast;
using Lumen.Features.Convert;
using Lumen.Features.Effects.Audio;
using Lumen.Features.Effects.Video;
using Lumen.Features.History;
using Lumen.Features.MediaInfo;
using Lumen.Features.Network;
using Lumen.Features.Playback.Engine;
using Lumen.Features.Playback.Video;
using Lumen.Features.Playlist.Files;
using Lumen.Features.Settings;
using Lumen.Features.Settings.Skins;
using Lumen.Features.Tools;
using Lumen.Features.Updates;
using Lumen.Features.Updates.Installers;
using Lumen.Infrastructure;

namespace Lumen.Composition;

/// <summary>
/// Compositieroot: hier worden alle services één keer aangemaakt. De viewmodels worden gebouwd in <see cref="ViewModelFactory"/>.
/// </summary>
public sealed class AppServices : IDisposable
{
    private readonly HttpClient _http = CreateHttpClient();

    public AppServices(Func<Window?> mainWindow)
    {
        MainWindow = mainWindow;

        Settings = new JsonSettingsStore(new JsonFileStore<AppSettings>(AppPaths.DataFile("settings.json")));
        History = new JsonHistoryStore(new JsonFileStore<List<RecentItem>>(AppPaths.DataFile("history.json")));
        Theme = new ThemeService(Settings);
        Skins = new SkinService(Settings);
        FilePicker = new FilePickerService(() => mainWindow());

        Engine = new VlcEngine();
        MediaPlayer = new VlcMediaPlayerService(Engine, VideoOutputModes.ForCurrentPlatform());
        VideoEffects = new VlcVideoEffects(Engine, MediaPlayer);
        AudioEffects = new VlcAudioEffects(Engine);
        MediaInfo = new VlcMediaInfoSource(Engine);
        Cast = new VlcCastService(Engine, MediaPlayer);
        Converter = new VlcConversionRunner(Engine);
        StreamResolver = new StreamResolver(Engine);
        PlaylistLoader = new PlaylistFileLoader(_http);
        Snapshots = new SnapshotService(VideoEffects);

        Updates = new UpdateService(
            new GitHubReleaseSource(_http),
            new UpdateDownloader(_http),
            UpdateInstallerFactory.ForCurrentPlatform());
    }

    public Func<Window?> MainWindow { get; }

    public ISettingsStore Settings { get; }
    public IHistoryStore History { get; }
    public ThemeService Theme { get; }
    public SkinService Skins { get; }
    public IFilePickerService FilePicker { get; }

    public VlcEngine Engine { get; }
    public VlcMediaPlayerService MediaPlayer { get; }
    public IVideoEffects VideoEffects { get; }
    public VlcAudioEffects AudioEffects { get; }
    public IMediaInfoSource MediaInfo { get; }
    public VlcCastService Cast { get; }
    public IConversionRunner Converter { get; }
    public StreamResolver StreamResolver { get; }
    public PlaylistFileLoader PlaylistLoader { get; }
    public SnapshotService Snapshots { get; }
    public UpdateService Updates { get; }

    public VideoOutput VideoOutput => MediaPlayer.Output;

    public void Dispose()
    {
        Cast.Dispose();
        MediaPlayer.Dispose();
        AudioEffects.Dispose();
        Engine.Dispose();
        _http.Dispose();
    }

    public static void ShutdownApp() =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();

    private static HttpClient CreateHttpClient()
    {
        var http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("Lumen");
        return http;
    }
}
