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

    /// <summary>Wat er nu geladen is: een bestandspad of een netwerkadres.</summary>
    string? CurrentSource { get; }

    void Open(string source, TimeSpan startAt);
    void Play();
    void Pause();
    void Stop();
    void Seek(TimeSpan position);
    void NextFrame();

    IReadOnlyList<TrackInfo> GetAudioTracks();
    IReadOnlyList<TrackInfo> GetSubtitleTracks();
    int CurrentAudioTrack { get; }
    int CurrentSubtitleTrack { get; }
    void SelectAudioTrack(int id);
    void SelectSubtitleTrack(int id);
    void LoadSubtitleFile(string path, bool select);
}

/// <summary>
/// Sommige instellingen (zoals roteren of casten) kan VLC alleen toepassen door het bestand opnieuw te openen.
/// </summary>
public interface IMediaReloader
{
    /// <summary>Zet (of wist, met null) een extra VLC-optie die bij elk geopend bestand meegegeven wordt.</summary>
    void SetMediaOption(string key, string? option);

    /// <summary>Opent het huidige bestand opnieuw op dezelfde positie, met de huidige opties.</summary>
    void Reload();
}
