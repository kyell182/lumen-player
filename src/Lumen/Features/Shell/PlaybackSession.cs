using Avalonia.Threading;
using Lumen.Features.History;
using Lumen.Features.Network;
using Lumen.Features.Playback;
using Lumen.Features.Playlist;
using Lumen.Features.Tools;

namespace Lumen.Features.Shell;

/// <summary>
/// Het afspelen van de afspeellijst: een item starten (hervatten waar je stopte), volgende/vorige,
/// wat er gebeurt als een bestand afloopt, en de voortgang bewaren.
/// </summary>
public sealed class PlaybackSession
{
    private static readonly TimeSpan RestartThreshold = TimeSpan.FromSeconds(5);

    private readonly PlayerViewModel _player;
    private readonly PlaylistViewModel _playlist;
    private readonly IHistoryStore _history;
    private readonly StreamResolver _resolver;
    private readonly SleepTimer _sleepTimer;
    private string? _currentSource;

    public PlaybackSession(
        PlayerViewModel player, PlaylistViewModel playlist, IHistoryStore history, StreamResolver resolver, SleepTimer sleepTimer)
    {
        _player = player;
        _playlist = playlist;
        _history = history;
        _resolver = resolver;
        _sleepTimer = sleepTimer;

        _player.MediaEnded += (_, _) => OnMediaEnded();
        _player.PlaybackPaused += (_, _) => SaveProgress();
        _playlist.ItemActivated += (_, item) => _ = PlayAsync(item);
    }

    /// <summary>Er start een nieuw item; de titel is meegegeven.</summary>
    public event EventHandler<string>? ItemStarted;

    /// <summary>Het oorspronkelijke adres of pad van wat nu speelt (niet het omgezette stream-adres).</summary>
    public string? CurrentSource => _currentSource;

    public async Task PlayAsync(PlaylistItemViewModel item)
    {
        SaveProgress();
        _playlist.Current = item;
        _currentSource = item.Source;
        ItemStarted?.Invoke(this, item.Title);

        var startAt = ResumePolicy.StartPositionFor(_history.Find(item.Source));
        var playable = await ResolveAsync(item.Source);

        // Eerst het videovlak laten verschijnen (lagere prioriteit dan de layout), dan pas afspelen.
        await Dispatcher.UIThread.InvokeAsync(() => _player.Open(playable, startAt), DispatcherPriority.Background);
    }

    public void Next()
    {
        if (_playlist.NextItem is { } next) _ = PlayAsync(next);
    }

    public void Previous()
    {
        if (_player.Position > RestartThreshold || _playlist.PreviousItem is not { } previous)
        {
            _player.SeekTo(0);
            return;
        }

        _ = PlayAsync(previous);
    }

    public void Stop()
    {
        SaveProgress();
        _player.Stop();
        _currentSource = null;
    }

    public void SaveProgress()
    {
        if (_currentSource is { } source && _player.DurationSeconds > 0)
        {
            _history.Record(source, _player.Position, _player.Duration);
        }
    }

    private void OnMediaEnded()
    {
        SaveProgress();
        if (_sleepTimer.ConsumeEndOfItem()) return;
        if (_playlist.NextAfterEnd is { } next) _ = PlayAsync(next);
    }

    private async Task<string> ResolveAsync(string source)
    {
        try
        {
            return await _resolver.ResolveAsync(source);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            Infrastructure.CrashLogger.Write($"Kon {source} niet omzetten naar een stream", ex);
            return source;
        }
    }
}
