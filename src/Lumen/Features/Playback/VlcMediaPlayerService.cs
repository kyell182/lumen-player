using System.Globalization;
using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;

namespace Lumen.Features.Playback;

/// <summary>
/// <see cref="IMediaPlayerService"/> op basis van libVLC.
/// Belangrijk: roep nooit libVLC aan vanuit een libVLC-event (deadlock) — de viewmodels marshalen eerst naar de UI-thread.
/// </summary>
public sealed class VlcMediaPlayerService : IMediaPlayerService, IVlcVideoSource
{
    private static readonly string[] EngineOptions =
    {
        "--no-video-title-show",
        "--audio-visual=visual",
        "--effect-list=spectrum",
    };

    private readonly LibVLC _libVlc;
    private readonly MediaPlayer _player;
    private Media? _media;

    public VlcMediaPlayerService()
    {
        _libVlc = new LibVLC(false, EngineOptions);
        _player = new MediaPlayer(_libVlc)
        {
            EnableHardwareDecoding = true,
            EnableKeyInput = false,
            EnableMouseInput = false,
        };
        HookEngineEvents();
    }

    public event EventHandler<PlaybackState>? StateChanged;
    public event EventHandler<TimeSpan>? PositionChanged;
    public event EventHandler<TimeSpan>? DurationChanged;

    public MediaPlayer Surface => _player;

    public PlaybackState State { get; private set; } = PlaybackState.Idle;

    public int Volume
    {
        get => _player.Volume;
        set => _player.Volume = Math.Clamp(value, 0, 100);
    }

    public bool IsMuted
    {
        get => _player.Mute;
        set => _player.Mute = value;
    }

    public float Rate
    {
        get => _player.Rate;
        set => _player.SetRate(value);
    }

    public int CurrentAudioTrack => _player.AudioTrack;

    public int CurrentSubtitleTrack => _player.Spu;

    public void Open(string path, TimeSpan startAt)
    {
        var media = new Media(_libVlc, new Uri(Path.GetFullPath(path)));
        if (startAt > TimeSpan.Zero)
        {
            media.AddOption(":start-time=" + startAt.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture));
        }

        _player.Play(media);
        _media?.Dispose();
        _media = media;
    }

    public void Play()
    {
        if (_media is null) return;

        if (State is PlaybackState.Ended or PlaybackState.Stopped or PlaybackState.Error)
        {
            _player.Stop();
            _player.Play();
            return;
        }

        _player.SetPause(false);
    }

    public void Pause() => _player.SetPause(true);

    public void Stop() => _player.Stop();

    public void Seek(TimeSpan position)
    {
        if (_player.IsSeekable)
        {
            _player.Time = (long)position.TotalMilliseconds;
        }
    }

    public IReadOnlyList<TrackInfo> GetAudioTracks() => ToTracks(_player.AudioTrackDescription);

    public IReadOnlyList<TrackInfo> GetSubtitleTracks() => ToTracks(_player.SpuDescription);

    public void SelectAudioTrack(int id) => _player.SetAudioTrack(id);

    public void SelectSubtitleTrack(int id) => _player.SetSpu(id);

    public void LoadSubtitleFile(string path) =>
        _player.AddSlave(MediaSlaveType.Subtitle, new Uri(Path.GetFullPath(path)).AbsoluteUri, true);

    public void Dispose()
    {
        _player.Stop();
        _media?.Dispose();
        _player.Dispose();
        _libVlc.Dispose();
    }

    private void HookEngineEvents()
    {
        _player.TimeChanged += (_, e) => PositionChanged?.Invoke(this, TimeSpan.FromMilliseconds(e.Time));
        _player.LengthChanged += (_, e) => DurationChanged?.Invoke(this, TimeSpan.FromMilliseconds(e.Length));
        _player.Opening += (_, _) => SetState(PlaybackState.Opening);
        _player.Playing += (_, _) => SetState(PlaybackState.Playing);
        _player.Paused += (_, _) => SetState(PlaybackState.Paused);
        _player.Stopped += (_, _) => SetState(PlaybackState.Stopped);
        _player.EndReached += (_, _) => SetState(PlaybackState.Ended);
        _player.EncounteredError += (_, _) => SetState(PlaybackState.Error);
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
