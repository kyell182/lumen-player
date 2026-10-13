using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lumen.Features.History;
using Lumen.Features.Home;
using Lumen.Features.Playback;
using Lumen.Features.Playlist;
using Lumen.Features.Settings;
using Lumen.Infrastructure;

namespace Lumen.Features.Shell;

/// <summary>Verbindt startscherm, speler en afspeellijst en regelt vensterbrede toestand.</summary>
public sealed partial class MainViewModel : ObservableObject
{
    private const string AppName = "Lumen";
    private static readonly TimeSpan RestartThreshold = TimeSpan.FromSeconds(5);

    private readonly IHistoryStore _history;
    private readonly ISettingsStore _settings;
    private readonly ThemeService _theme;
    private readonly IFilePickerService _picker;
    private readonly ControlsAutoHider _autoHider;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsControlBarVisible), nameof(IsPlaylistPanelVisible))]
    private bool _isHomeVisible = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsControlBarVisible), nameof(IsRevealStripVisible))]
    private bool _isFullScreen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsControlBarVisible), nameof(IsRevealStripVisible))]
    private bool _controlsVisible = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPlaylistPanelVisible))]
    private bool _isPlaylistOpen;

    [ObservableProperty]
    private string _currentTitle = string.Empty;

    [ObservableProperty]
    private string _windowTitle = AppName;

    public MainViewModel(
        PlayerViewModel player,
        PlaylistViewModel playlist,
        HomeViewModel home,
        IHistoryStore history,
        ISettingsStore settings,
        ThemeService theme,
        IFilePickerService picker)
    {
        Player = player;
        Playlist = playlist;
        Home = home;
        _history = history;
        _settings = settings;
        _theme = theme;
        _picker = picker;
        _autoHider = new ControlsAutoHider(TimeSpan.FromSeconds(2.5), HideControlsWhenIdle);

        WireEvents();
    }

    public PlayerViewModel Player { get; }

    public PlaylistViewModel Playlist { get; }

    public HomeViewModel Home { get; }

    public bool IsControlBarVisible => !IsHomeVisible && (ControlsVisible || !IsFullScreen);

    public bool IsRevealStripVisible => IsFullScreen && !ControlsVisible;

    public bool IsPlaylistPanelVisible => IsPlaylistOpen && !IsHomeVisible;

    // ----- Openen -----

    public void OpenStartupFiles(IEnumerable<string> args) => PlayFiles(args.ToList());

    public void HandleDroppedPaths(IReadOnlyList<string> paths)
    {
        var subtitle = paths.FirstOrDefault(MediaFileTypes.IsSubtitle);
        if (subtitle is not null && !IsHomeVisible)
        {
            Player.LoadSubtitle(subtitle);
            return;
        }

        PlayFiles(paths);
    }

    public void PlayFiles(IReadOnlyList<string> paths)
    {
        var files = MediaFileTypes.Expand(paths);
        if (Playlist.Replace(files) is { } first)
        {
            PlayItem(first);
        }
    }

    [RelayCommand]
    private async Task OpenFilesAsync()
    {
        var files = await _picker.PickMediaFilesAsync();
        if (files.Count > 0) PlayFiles(files);
    }

    [RelayCommand]
    private async Task OpenFolderAsync()
    {
        if (await _picker.PickFolderAsync() is { } folder) PlayFiles(new[] { folder });
    }

    [RelayCommand]
    private async Task LoadSubtitleFileAsync()
    {
        if (await _picker.PickSubtitleAsync() is { } path) Player.LoadSubtitle(path);
    }

    // ----- Navigatie in de afspeellijst -----

    [RelayCommand]
    private void Next()
    {
        if (Playlist.NextItem is { } next) PlayItem(next);
    }

    [RelayCommand]
    private void Previous()
    {
        if (Player.Position > RestartThreshold || Playlist.PreviousItem is null)
        {
            Player.SeekTo(0);
            return;
        }

        PlayItem(Playlist.PreviousItem);
    }

    [RelayCommand]
    private void GoHome()
    {
        SaveProgress();
        Player.Stop();
        IsFullScreen = false;
        IsHomeVisible = true;
        SetTitle(string.Empty);
        Home.Refresh();
    }

    // ----- Venster -----

    [RelayCommand]
    private void ToggleFullScreen() => IsFullScreen = !IsFullScreen;

    [RelayCommand]
    private void TogglePlaylist() => IsPlaylistOpen = !IsPlaylistOpen;

    [RelayCommand]
    private void ToggleTheme() => _theme.Toggle();

    public void ExitFullScreen() => IsFullScreen = false;

    /// <summary>Muis of toetsenbord gebruikt: bediening tonen en afteltimer herstarten.</summary>
    public void ReportUserActivity()
    {
        ControlsVisible = true;
        if (IsFullScreen) _autoHider.Restart();
    }

    public void Shutdown()
    {
        SaveProgress();
        _settings.Current.Volume = Player.Volume;
        _settings.Save();
        _autoHider.Stop();
    }

    partial void OnIsFullScreenChanged(bool value)
    {
        ControlsVisible = true;
        if (value) _autoHider.Restart();
        else _autoHider.Stop();
    }

    // ----- Intern -----

    private void WireEvents()
    {
        Player.MediaEnded += (_, _) => OnMediaEnded();
        Player.PlaybackPaused += (_, _) => SaveProgress();
        Playlist.ItemActivated += (_, item) => PlayItem(item);
        Home.OpenFilesRequested += (_, _) => OpenFilesCommand.Execute(null);
        Home.OpenFolderRequested += (_, _) => OpenFolderCommand.Execute(null);
        Home.RecentRequested += (_, path) => PlayFiles(new[] { path });
    }

    private void PlayItem(PlaylistItemViewModel item)
    {
        SaveProgress();
        Playlist.Current = item;
        SetTitle(item.Title);

        var startAt = ResumePolicy.StartPositionFor(_history.Find(item.Path));
        IsHomeVisible = false;

        // Eerst het videovlak laten verschijnen, anders opent VLC een eigen venster.
        Dispatcher.UIThread.Post(() => Player.Open(item.Path, startAt), DispatcherPriority.Background);
    }

    private void OnMediaEnded()
    {
        SaveProgress();
        if (Playlist.NextItem is { } next)
        {
            PlayItem(next);
        }
        else
        {
            ControlsVisible = true;
        }
    }

    private void SaveProgress()
    {
        if (Player.CurrentPath is { } path && Player.DurationSeconds > 0)
        {
            _history.Record(path, Player.Position, Player.Duration);
        }
    }

    private void HideControlsWhenIdle()
    {
        if (IsFullScreen && Player.IsPlaying) ControlsVisible = false;
    }

    private void SetTitle(string title)
    {
        CurrentTitle = title;
        WindowTitle = string.IsNullOrEmpty(title) ? AppName : $"{title} — {AppName}";
    }
}
