using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lumen.Features.Playback.Subtitles;

namespace Lumen.Features.Playback;

/// <summary>Toestand van het afspelen (positie, volume, snelheid, sporen) voor de bedieningsbalk.</summary>
public sealed partial class PlayerViewModel : ObservableObject
{
    public static readonly IReadOnlyList<float> PlaybackRates = new[] { 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f };

    private const double SkipSeconds = 10;

    private readonly IMediaPlayerService _player;
    private readonly PlaybackClock _clock = new();
    private bool _isUserSeeking;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private double _positionSeconds;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DurationText))]
    private double _durationSeconds;

    [ObservableProperty]
    private string _positionText = "0:00";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowMutedIcon))]
    private double _volume;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowMutedIcon))]
    private bool _isMuted;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RateText))]
    private float _rate = 1f;

    public PlayerViewModel(IMediaPlayerService player, double initialVolume)
    {
        _player = player;
        Subtitles = new SubtitlesViewModel(player, _clock);
        Volume = Math.Clamp(initialVolume, 0, 100);

        _player.StateChanged += (_, state) => Dispatcher.UIThread.Post(() => OnEngineStateChanged(state));
        _player.PositionChanged += (_, position) => Dispatcher.UIThread.Post(() => OnEnginePositionChanged(position));
        _player.DurationChanged += (_, duration) => Dispatcher.UIThread.Post(() => DurationSeconds = duration.TotalSeconds);
    }

    public event EventHandler? MediaEnded;
    public event EventHandler? PlaybackPaused;

    public SubtitlesViewModel Subtitles { get; }

    public string? CurrentPath { get; private set; }

    public string DurationText => TimeFormat.Format(DurationSeconds);

    public string RateText => TimeFormat.FormatRate(Rate);

    public bool ShowMutedIcon => IsMuted || Volume <= 0;

    public TimeSpan Position => TimeSpan.FromSeconds(PositionSeconds);

    public TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);

    public void Open(string path, TimeSpan startAt)
    {
        CurrentPath = path;
        DurationSeconds = 0;
        PositionSeconds = startAt.TotalSeconds;
        _clock.Sync(startAt);
        Subtitles.PrepareFor(path);
        _player.Open(path, startAt);
    }

    public void Stop()
    {
        _player.Stop();
        Subtitles.Stop();
        CurrentPath = null;
        IsPlaying = false;
        PositionSeconds = 0;
        DurationSeconds = 0;
    }

    [RelayCommand]
    public void TogglePlayPause()
    {
        if (IsPlaying) _player.Pause();
        else _player.Play();
    }

    [RelayCommand]
    private void SkipForward() => SeekBy(SkipSeconds);

    [RelayCommand]
    private void SkipBackward() => SeekBy(-SkipSeconds);

    [RelayCommand]
    private void ToggleMute() => IsMuted = !IsMuted;

    public void SeekBy(double seconds) => SeekTo(PositionSeconds + seconds);

    public void SeekTo(double seconds)
    {
        var target = Math.Clamp(seconds, 0, Math.Max(0, DurationSeconds));
        PositionSeconds = target;
        _clock.Sync(TimeSpan.FromSeconds(target));
        _player.Seek(TimeSpan.FromSeconds(target));
    }

    public void BeginUserSeek() => _isUserSeeking = true;

    public void EndUserSeek()
    {
        if (!_isUserSeeking) return;
        _isUserSeeking = false;
        SeekTo(PositionSeconds);
    }

    public void ChangeVolume(double delta) => Volume = Math.Clamp(Volume + delta, 0, 100);

    public void StepRate(int direction)
    {
        var rates = PlaybackRates.ToList();
        var index = rates.FindIndex(r => Math.Abs(r - Rate) < 0.01f);
        if (index < 0) index = rates.IndexOf(1f);
        Rate = rates[Math.Clamp(index + direction, 0, rates.Count - 1)];
    }

    public IReadOnlyList<TrackInfo> GetAudioTracks() => _player.GetAudioTracks();

    public int CurrentAudioTrack => _player.CurrentAudioTrack;

    public void SelectAudioTrack(int id) => _player.SelectAudioTrack(id);

    public void LoadSubtitle(string path) => Subtitles.AddFile(path);

    partial void OnVolumeChanged(double value)
    {
        _player.Volume = (int)Math.Round(value);
        if (value > 0 && IsMuted) IsMuted = false;
    }

    partial void OnIsMutedChanged(bool value) => _player.IsMuted = value;

    partial void OnRateChanged(float value)
    {
        _player.Rate = value;
        _clock.SetRate(value);
    }

    partial void OnIsPlayingChanged(bool value) => _clock.SetRunning(value);

    partial void OnPositionSecondsChanged(double value) => PositionText = TimeFormat.Format(value);

    private void OnEngineStateChanged(PlaybackState state)
    {
        IsPlaying = state == PlaybackState.Playing;

        switch (state)
        {
            case PlaybackState.Playing:
                ApplyAudioSettings();
                Subtitles.OnPlaybackStarted();
                break;
            case PlaybackState.Paused:
                PlaybackPaused?.Invoke(this, EventArgs.Empty);
                break;
            case PlaybackState.Ended:
                PositionSeconds = DurationSeconds;
                MediaEnded?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    private void OnEnginePositionChanged(TimeSpan position)
    {
        _clock.Sync(position);
        if (!_isUserSeeking) PositionSeconds = position.TotalSeconds;
    }

    // Volume en mute worden pas betrouwbaar toegepast zodra de audio-uitvoer bestaat.
    private void ApplyAudioSettings()
    {
        _player.Volume = (int)Math.Round(Volume);
        _player.IsMuted = IsMuted;
        if (Math.Abs(_player.Rate - Rate) > 0.01f) _player.Rate = Rate;
    }
}
