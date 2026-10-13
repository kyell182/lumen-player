using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lumen.Features.History;
using Lumen.Features.Home;
using Lumen.Features.Playback;
using Lumen.Features.Playlist;
using Lumen.Features.Settings;
using Lumen.Features.Settings.Skins;
using Lumen.Features.Updates;
using Lumen.Infrastructure;

namespace Lumen.Features.Shell;

/// <summary>Verbindt startscherm, speler en afspeellijst en regelt vensterbrede toestand.</summary>
public sealed partial class MainViewModel : ObservableObject
{
    private const string AppName = "Lumen";
    private static readonly TimeSpan RestartThreshold = TimeSpan.FromSeconds(5);

    private readonly IHistoryStore _history;
    private readonly ISettingsStore _settings;
    private readonly IFilePickerService _picker;
    private readonly ControlsAutoHider _autoHider;
    private bool _isPointerOverControls;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPlaylistPanelVisible))]
    private bool _isHomeVisible = true;

    [ObservableProperty]
    private bool _isFullScreen;

    [ObservableProperty]
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
        UpdateViewModel update,
        IHistoryStore history,
        ISettingsStore settings,
        ThemeService theme,
        SkinService skins,
        IFilePickerService picker)
    {
        Player = player;
        Playlist = playlist;
        Home = home;
        Update = update;
        _history = history;
        _settings = settings;
        Theme = theme;
        Skins = skins;
        _picker = picker;
        _autoHider = new ControlsAutoHider(TimeSpan.FromSeconds(2.5), HideControlsWhenIdle);

        WireEvents();
    }

    public PlayerViewModel Player { get; }

    public PlaylistViewModel Playlist { get; }

    public HomeViewModel Home { get; }

    public UpdateViewModel Update { get; }

    /// <summary>Licht of donker.</summary>
    public ThemeService Theme { get; }

    /// <summary>Kleurthema (accentkleur).</summary>
    public SkinService Skins { get; }

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
        _autoHider.Stop();
        ControlsVisible = true;
        SetTitle(string.Empty);
        Home.Refresh();
    }

    // ----- Venster -----

    [RelayCommand]
    private void ToggleFullScreen() => IsFullScreen = !IsFullScreen;

    [RelayCommand]
    private void TogglePlaylist() => IsPlaylistOpen = !IsPlaylistOpen;

    public void ExitFullScreen() => IsFullScreen = false;

    /// <summary>Muis of toetsenbord gebruikt: bediening tonen en afteltimer herstarten.</summary>
    public void ReportUserActivity()
    {
        ControlsVisible = true;
        if (!IsHomeVisible) _autoHider.Restart();
    }

    /// <summary>Zolang de muis op de bediening staat, blijft die zichtbaar.</summary>
    public void SetPointerOverControls(bool isOver)
    {
        _isPointerOverControls = isOver;
        ReportUserActivity();
    }

    public void Shutdown()
    {
        SaveProgress();
        _settings.Current.Volume = Player.Volume;
        _settings.Save();
        _autoHider.Stop();
    }

    partial void OnIsFullScreenChanged(bool value) => ReportUserActivity();

    // ----- Intern -----

    private void WireEvents()
    {
        Player.MediaEnded += (_, _) => OnMediaEnded();
        Player.PlaybackPaused += (_, _) => SaveProgress();
        Player.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PlayerViewModel.IsPlaying)) ReportUserActivity();
        };
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
        if (!IsHomeVisible && Player.IsPlaying && !_isPointerOverControls) ControlsVisible = false;
    }

    private void SetTitle(string title)
    {
        CurrentTitle = title;
        WindowTitle = string.IsNullOrEmpty(title) ? AppName : $"{title} — {AppName}";
    }
}
