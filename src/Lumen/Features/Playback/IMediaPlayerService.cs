namespace Lumen.Features.Playback;

/// <summary>
/// Afspeelcontract dat losstaat van de gebruikte engine.
/// Let op: events kunnen op een achtergrondthread binnenkomen.
/// </summary>
public interface IMediaPlayerService : IDisposable
{
    event EventHandler<PlaybackState>? StateChanged;
    event EventHandler<TimeSpan>? PositionChanged;
    event EventHandler<TimeSpan>? DurationChanged;

    PlaybackState State { get; }
    int Volume { get; set; }
    bool IsMuted { get; set; }
    float Rate { get; set; }

    void Open(string path, TimeSpan startAt);
    void Play();
    void Pause();
    void Stop();
    void Seek(TimeSpan position);

    IReadOnlyList<TrackInfo> GetAudioTracks();
    IReadOnlyList<TrackInfo> GetSubtitleTracks();
    int CurrentAudioTrack { get; }
    int CurrentSubtitleTrack { get; }
    void SelectAudioTrack(int id);
    void SelectSubtitleTrack(int id);
    void LoadSubtitleFile(string path, bool select);
}
