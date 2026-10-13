using System.Globalization;
using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using Lumen.Features.Playback.Video;
using Lumen.Infrastructure;

namespace Lumen.Features.Playback.Engine;

/// <summary>
/// <see cref="IMediaPlayerService"/> op basis van libVLC.
/// Belangrijk: roep nooit libVLC aan vanuit een libVLC-event (deadlock) — de viewmodels marshalen eerst naar de UI-thread.
/// </summary>
public sealed class VlcMediaPlayerService : IMediaPlayerService, IMediaReloader
{
    private readonly VlcEngine _engine;
    private readonly VlcVideoFrameSource? _frames;
    private readonly VlcNativeWindowTarget? _nativeTarget;
    private readonly Dictionary<string, string> _mediaOptions = new();
    private Media? _media;

    public VlcMediaPlayerService(VlcEngine engine, VideoOutputMode outputMode)
    {
        _engine = engine;
        if (outputMode == VideoOutputMode.Frames) _frames = new VlcVideoFrameSource(Player);
        else _nativeTarget = new VlcNativeWindowTarget(Player);
        HookEngineEvents();
    }

    public event EventHandler<PlaybackState>? StateChanged;
    public event EventHandler<TimeSpan>? PositionChanged;
    public event EventHandler<TimeSpan>? DurationChanged;

    /// <summary>Waar de video naartoe gaat: frames voor Lumen, of een native venster.</summary>
    public VideoOutput Output => new(_frames, _nativeTarget);

    public PlaybackState State { get; private set; } = PlaybackState.Idle;

    public string? CurrentSource { get; private set; }

    public int Volume
    {
        get => Player.Volume;
        set => Player.Volume = Math.Clamp(value, 0, 100);
    }

    public bool IsMuted
    {
        get => Player.Mute;
        set => Player.Mute = value;
    }

    public float Rate
    {
        get => Player.Rate;
        set => Player.SetRate(value);
    }

    public int CurrentAudioTrack => Player.AudioTrack;

    public int CurrentSubtitleTrack => Player.Spu;

    private MediaPlayer Player => _engine.Player;

    public void Open(string source, TimeSpan startAt)
    {
        CurrentSource = source;
        var media = new Media(_engine.LibVlc, MediaSource.ToUri(source));
        foreach (var option in _mediaOptions.Values) media.AddOption(option);
        if (startAt > TimeSpan.Zero)
        {
            media.AddOption(":start-time=" + startAt.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture));
        }

        _media?.Dispose();
        _media = media;

        if (_nativeTarget is { } target) target.RunWhenReady(() => Player.Play(media));
        else Player.Play(media);
    }

    public void SetMediaOption(string key, string? option)
    {
        if (option is null) _mediaOptions.Remove(key);
        else _mediaOptions[key] = option;
    }

    public void Reload()
    {
        if (CurrentSource is not { } source) return;

        var position = TimeSpan.FromMilliseconds(Math.Max(0, Player.Time));
        var wasPaused = State == PlaybackState.Paused;
        Player.Stop();
        Open(source, position);
        if (wasPaused) Player.SetPause(true);
    }

    public void Play()
    {
        if (_media is null) return;

        if (State is PlaybackState.Ended or PlaybackState.Stopped or PlaybackState.Error)
        {
            Player.Stop();
            Player.Play();
            return;
        }

        Player.SetPause(false);
    }

    public void Pause() => Player.SetPause(true);

    public void Stop()
    {
        Player.Stop();
        CurrentSource = null;
    }

    public void Seek(TimeSpan position)
    {
        if (Player.IsSeekable)
        {
            Player.Time = (long)position.TotalMilliseconds;
        }
    }

    public void NextFrame() => Player.NextFrame();

    public IReadOnlyList<TrackInfo> GetAudioTracks() => ToTracks(Player.AudioTrackDescription);

    public IReadOnlyList<TrackInfo> GetSubtitleTracks() => ToTracks(Player.SpuDescription);

    public void SelectAudioTrack(int id) => Player.SetAudioTrack(id);

    public void SelectSubtitleTrack(int id) => Player.SetSpu(id);

    public void LoadSubtitleFile(string path, bool select) =>
        Player.AddSlave(MediaSlaveType.Subtitle, new Uri(Path.GetFullPath(path)).AbsoluteUri, select);

    public void Dispose()
    {
        Player.Stop();
        _media?.Dispose();
        _frames?.Dispose();
    }

    private void HookEngineEvents()
    {
        Player.TimeChanged += (_, e) => PositionChanged?.Invoke(this, TimeSpan.FromMilliseconds(e.Time));
        Player.LengthChanged += (_, e) => DurationChanged?.Invoke(this, TimeSpan.FromMilliseconds(e.Length));
        Player.Opening += (_, _) => SetState(PlaybackState.Opening);
        Player.Playing += (_, _) => SetState(PlaybackState.Playing);
        Player.Paused += (_, _) => SetState(PlaybackState.Paused);
        Player.Stopped += (_, _) => SetState(PlaybackState.Stopped);
        Player.EndReached += (_, _) => SetState(PlaybackState.Ended);
        Player.EncounteredError += (_, _) => SetState(PlaybackState.Error);
        // Niet vanuit het VLC-event zelf aanroepen (deadlock-gevaar), dus via de threadpool.
        if (_frames is not null)
        {
            Player.Vout += (_, _) => ThreadPool.QueueUserWorkItem(_ => UpdateVisibleVideoSize());
        }
    }

    private void UpdateVisibleVideoSize()
    {
        uint width = 0, height = 0;
        if (Player.Size(0, ref width, ref height))
        {
            _frames?.SetVisibleSize((int)width, (int)height);
        }
    }

    private void SetState(PlaybackState state)
    {
        State = state;
        StateChanged?.Invoke(this, state);
    }

    private static IReadOnlyList<TrackInfo> ToTracks(IEnumerable<TrackDescription>? tracks) =>
        tracks?.Select(t => new TrackInfo(t.Id, string.IsNullOrWhiteSpace(t.Name) ? $"Spoor {t.Id}" : t.Name)).ToList()
        ?? (IReadOnlyList<TrackInfo>)Array.Empty<TrackInfo>();
}
