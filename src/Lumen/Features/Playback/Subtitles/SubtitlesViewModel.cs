using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Lumen.Features.Playback.Subtitles.Parsing;
using Lumen.Features.Playback.Tracks;

namespace Lumen.Features.Playback.Subtitles;

/// <summary>
/// Beheert welke ondertitel getoond wordt. Losse bestanden tekent Lumen zelf (werkt altijd, ook met
/// GPU-decodering); ondertitels die in de video zitten laat Lumen door VLC tekenen.
/// </summary>
public sealed partial class SubtitlesViewModel : ObservableObject
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(50);
    private static readonly TimeSpan EmbeddedTracksSettleDelay = TimeSpan.FromMilliseconds(800);

    private readonly IMediaPlayerService _player;
    private readonly PlaybackClock _clock;
    private readonly DispatcherTimer _ticker;

    private List<ExternalSubtitle> _files = new();
    private ExternalSubtitle? _activeFile;
    private string _videoName = string.Empty;
    private bool _needsInitialChoice;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasText))]
    private string _currentText = string.Empty;

    public SubtitlesViewModel(IMediaPlayerService player, PlaybackClock clock)
    {
        _player = player;
        _clock = clock;
        _ticker = new DispatcherTimer { Interval = TickInterval };
        _ticker.Tick += (_, _) => RefreshText();
    }

    public bool HasText => CurrentText.Length > 0;

    /// <summary>Zoekt ondertitelbestanden bij een nieuwe video. De keuze volgt zodra die speelt.</summary>
    public void PrepareFor(string videoPath)
    {
        _videoName = Path.GetFileNameWithoutExtension(videoPath);
        _files = SubtitleFinder.FindFor(videoPath).Select(LoadFile).OfType<ExternalSubtitle>().ToList();
        _activeFile = null;
        _needsInitialChoice = true;
        CurrentText = string.Empty;
    }

    public void OnPlaybackStarted()
    {
        _ticker.Start();
        if (!_needsInitialChoice) return;

        _needsInitialChoice = false;
        if (_files.Count > 0) SelectFile(_files[0]);
        else _ = SelectPreferredEmbeddedAsync();
    }

    public void Stop()
    {
        _ticker.Stop();
        _activeFile = null;
        CurrentText = string.Empty;
    }

    public IReadOnlyList<SubtitleOption> GetOptions()
    {
        var embeddedId = _activeFile is null ? _player.CurrentSubtitleTrack : TrackInfo.DisabledId;
        var options = new List<SubtitleOption>
        {
            new(SubtitleSource.Off, "Uit", _activeFile is null && embeddedId == TrackInfo.DisabledId),
        };

        options.AddRange(_player.GetSubtitleTracks()
            .Where(t => !t.IsDisabledTrack)
            .Select(t => new SubtitleOption(SubtitleSource.Embedded, TrackLabel.ForEngineTrack(t.Name, t.Id), t.Id == embeddedId, t.Id)));

        options.AddRange(_files.Select(f => new SubtitleOption(SubtitleSource.File, f.Label, f == _activeFile, File: f)));
        return MakeLabelsUnique(options);
    }

    public void Select(SubtitleOption option)
    {
        switch (option.Source)
        {
            case SubtitleSource.File when option.File is not null:
                SelectFile(option.File);
                break;
            case SubtitleSource.Embedded:
                _activeFile = null;
                _player.SelectSubtitleTrack(option.EmbeddedId);
                break;
            default:
                _activeFile = null;
                _player.SelectSubtitleTrack(TrackInfo.DisabledId);
                break;
        }

        RefreshText();
    }

    /// <summary>Een bestand dat de gebruiker zelf koos of op de video sleepte.</summary>
    public void AddFile(string path)
    {
        if (LoadFile(path) is { } file)
        {
            _files.Insert(0, file);
            SelectFile(file);
        }
        else
        {
            // Formaat dat Lumen zelf niet leest (bv. .sub): laat VLC het proberen.
            _player.LoadSubtitleFile(path, select: true);
        }
    }

    private void SelectFile(ExternalSubtitle file)
    {
        _player.SelectSubtitleTrack(TrackInfo.DisabledId);
        _activeFile = file;
        RefreshText();
    }

    // Zitten er Nederlandse ondertitels in de video, dan die kiezen. VLC kent de sporen pas even na de start.
    private async Task SelectPreferredEmbeddedAsync()
    {
        await Task.Delay(EmbeddedTracksSettleDelay);
        if (_activeFile is not null) return;

        var dutch = _player.GetSubtitleTracks()
            .Where(t => !t.IsDisabledTrack)
            .Select(t => (Track: t, Label: TrackLabel.ForEngineTrack(t.Name, t.Id)))
            .FirstOrDefault(t => t.Label == LanguageNames.Dutch || t.Label.StartsWith(LanguageNames.Dutch + " (SDH", StringComparison.Ordinal));

        if (dutch.Track is not null) _player.SelectSubtitleTrack(dutch.Track.Id);
    }

    private void RefreshText() =>
        CurrentText = _activeFile?.Document.TextAt(_clock.Now) ?? string.Empty;

    private ExternalSubtitle? LoadFile(string path) =>
        SubtitleLoader.TryLoad(path) is { } document
            ? new ExternalSubtitle(path, TrackLabel.ForFile(path, _videoName), document)
            : null;

    private static IReadOnlyList<SubtitleOption> MakeLabelsUnique(List<SubtitleOption> options)
    {
        var seen = new Dictionary<string, int>();
        return options.Select(o =>
        {
            var count = seen[o.Label] = seen.GetValueOrDefault(o.Label) + 1;
            return count == 1 ? o : o with { Label = $"{o.Label} {count}" };
        }).ToList();
    }
}
