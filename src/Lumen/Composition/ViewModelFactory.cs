using Lumen.Features.Cast;
using Lumen.Features.Convert;
using Lumen.Features.Effects;
using Lumen.Features.Effects.Audio;
using Lumen.Features.Effects.Video;
using Lumen.Features.Home;
using Lumen.Features.MediaInfo;
using Lumen.Features.Playback;
using Lumen.Features.Playlist;
using Lumen.Features.Shell;
using Lumen.Features.Tools;
using Lumen.Features.Updates;

namespace Lumen.Composition;

/// <summary>Bouwt de viewmodels en koppelt ze aan de services.</summary>
public static class ViewModelFactory
{
    public static MainViewModel CreateMain(AppServices services)
    {
        var player = new PlayerViewModel(services.MediaPlayer, services.Settings.Current.Volume);
        var playlist = new PlaylistViewModel(services.FilePicker);
        var sleepTimer = new SleepTimer();
        var session = new PlaybackSession(player, playlist, services.History, services.StreamResolver, sleepTimer);

        var tools = new ToolsViewModel(
            player,
            services.Snapshots,
            sleepTimer,
            new StreamRecorder(services.Converter),
            new Notifier(),
            () => session.CurrentSource);

        var parts = new MainViewModelParts(
            player,
            playlist,
            new HomeViewModel(services.History),
            new UpdateViewModel(services.Updates, AppServices.ShutdownApp),
            CreateEffects(services, player),
            new MediaInfoViewModel(services.MediaInfo),
            new CastViewModel(services.Cast),
            tools,
            services.Theme,
            services.Skins);

        var dialogs = new DialogService(services.MainWindow, new ConvertViewModel(services.Converter, services.FilePicker));
        var opener = new MediaOpener(services.PlaylistLoader);

        return new MainViewModel(parts, session, opener, dialogs, services.Settings, services.FilePicker);
    }

    private static EffectsViewModel CreateEffects(AppServices services, PlayerViewModel player) => new(
        new VideoEffectsViewModel(services.VideoEffects),
        new EqualizerViewModel(services.AudioEffects),
        new SyncViewModel(services.AudioEffects, player.Subtitles));
}
