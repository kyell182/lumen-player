using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lumen.Features.Cast;
using Lumen.Features.Effects;
using Lumen.Features.Home;
using Lumen.Features.MediaInfo;
using Lumen.Features.Playback;
using Lumen.Features.Playlist;
using Lumen.Features.Settings;
using Lumen.Features.Settings.Skins;
using Lumen.Features.Tools;
using Lumen.Features.Updates;
using Lumen.Infrastructure;

namespace Lumen.Features.Shell;

/// <summary>Het hoofdvenster: wat er zichtbaar is (startscherm, video, panelen) en de openen-acties.</summary>
public sealed partial class MainViewModel : ObservableObject
{
    private const string AppName = "Lumen";

    private readonly PlaybackSession _session;
    private readonly MediaOpener _opener;
    private readonly IDialogService _dialogs;
    private readonly ISettingsStore _settings;
    private readonly IFilePickerService _picker;
    private readonly ControlsAutoHider _autoHider;
    private bool _isPointerOverControls;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPlaylistPanelVisible), nameof(IsEffectsPanelVisible), nameof(IsInfoPanelVisible))]
    private bool _isHomeVisible = true;

    [ObservableProperty]
    private bool _isFullScreen;

    [ObservableProperty]
    private bool _controlsVisible = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPlaylistPanelVisible), nameof(IsEffectsPanelVisible), nameof(IsInfoPanelVisible))]
    private SidePanel _activePanel;

    [ObservableProperty]
    private string _currentTitle = string.Empty;

    [ObservableProperty]
    private string _windowTitle = AppName;

    public MainViewModel(MainViewModelParts parts, PlaybackSession session, MediaOpener opener, IDialogService dialogs,
        ISettingsStore settings, IFilePickerService picker)
    {
        Player = parts.Player;
        Playlist = parts.Playlist;
        Home = parts.Home;
        Update = parts.Update;
        Effects = parts.Effects;
        MediaInfo = parts.MediaInfo;
        Cast = parts.Cast;
        Tools = parts.Tools;
        Theme = parts.Theme;
        Skins = parts.Skins;
        _session = session;
        _opener = opener;
        _dialogs = dialogs;
        _settings = settings;
        _picker = picker;
        _autoHider = new ControlsAutoHider(TimeSpan.FromSeconds(2.5), HideControlsWhenIdle);

        WireEvents();
    }

    public PlayerViewModel Player { get; }
    public PlaylistViewModel Playlist { get; }
    public HomeViewModel Home { get; }
    public UpdateViewModel Update { get; }
    public EffectsViewModel Effects { get; }
    public MediaInfoViewModel MediaInfo { get; }
    public CastViewModel Cast { get; }
    public ToolsViewModel Tools { get; }

    /// <summary>Licht of donker.</summary>
    public ThemeService Theme { get; }

    /// <summary>Kleurthema (accentkleur).</summary>
    public SkinService Skins { get; }

    public bool IsPlaylistPanelVisible => IsPanelVisible(SidePanel.Playlist);
    public bool IsEffectsPanelVisible => IsPanelVisible(SidePanel.Effects);
    public bool IsInfoPanelVisible => IsPanelVisible(SidePanel.Info);

    // ----- Openen -----

    public void OpenStartupFiles(IEnumerable<string> args) => _ = PlayAsync(args.ToList());

    public void HandleDroppedPaths(IReadOnlyList<string> paths)
    {
        var subtitle = paths.FirstOrDefault(MediaFileTypes.IsSubtitle);
        if (subtitle is not null && !IsHomeVisible)
        {
            Player.LoadSubtitle(subtitle);
            return;
        }

        _ = PlayAsync(paths);
    }

    public async Task PlayAsync(IReadOnlyList<string> inputs)
    {
        var entries = await _opener.ToEntriesAsync(inputs);
        if (Playlist.Replace(entries) is { } first) await _session.PlayAsync(first);
        else if (inputs.Count > 0) Tools.Notifier.Show("Hier zit niets in dat Lumen kan afspelen");
    }

    [RelayCommand]
    private async Task OpenFilesAsync()
    {
        var files = await _picker.PickMediaFilesAsync();
        if (files.Count > 0) await PlayAsync(files);
    }

    [RelayCommand]
    private async Task OpenFolderAsync()
    {
        if (await _picker.PickFolderAsync() is { } folder) await PlayAsync(new[] { folder });
    }

    [RelayCommand]
    private async Task OpenPlaylistAsync()
    {
        if (await _picker.PickPlaylistAsync() is { } playlist) await PlayAsync(new[] { playlist });
    }

    [RelayCommand]
    private async Task OpenStreamAsync()
    {
        if (await _dialogs.AskStreamUrlAsync() is { } url) await PlayAsync(new[] { url });
    }

    [RelayCommand]
    private void ShowConverter() => _dialogs.ShowConverter();

    [RelayCommand]
    private async Task LoadSubtitleFileAsync()
    {
        if (await _picker.PickSubtitleAsync() is { } path) Player.LoadSubtitle(path);
    }

    // ----- Afspeellijst -----

    [RelayCommand]
    private void Next() => _session.Next();

    [RelayCommand]
    private void Previous() => _session.Previous();

    [RelayCommand]
    private void GoHome()
    {
        _session.Stop();
        IsFullScreen = false;
        IsHomeVisible = true;
        _autoHider.Stop();
        ControlsVisible = true;
        SetTitle(string.Empty);
        Home.Refresh();
    }

    // ----- Venster en panelen -----

    [RelayCommand]
    private void ToggleFullScreen() => IsFullScreen = !IsFullScreen;

    [RelayCommand]
    private void TogglePlaylist() => TogglePanel(SidePanel.Playlist);

    [RelayCommand]
    private void ToggleEffects() => TogglePanel(SidePanel.Effects);

    [RelayCommand]
    private void ToggleInfo() => TogglePanel(SidePanel.Info);

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
        _session.SaveProgress();
        _settings.Current.Volume = Player.Volume;
        _settings.Save();
        _autoHider.Stop();
    }

    partial void OnIsFullScreenChanged(bool value) => ReportUserActivity();

    partial void OnActivePanelChanged(SidePanel value)
    {
        if (value == SidePanel.Info) MediaInfo.Start();
        else MediaInfo.Stop();
    }

    // ----- Intern -----

    private void WireEvents()
    {
        _session.ItemStarted += (_, title) =>
        {
            SetTitle(title);
            IsHomeVisible = false;
        };
        Player.PlaybackStarted += (_, _) => Effects.ReapplyAfterMediaChange();
        Player.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PlayerViewModel.IsPlaying)) ReportUserActivity();
        };
        Home.OpenFilesRequested += (_, _) => OpenFilesCommand.Execute(null);
        Home.OpenFolderRequested += (_, _) => OpenFolderCommand.Execute(null);
        Home.OpenStreamRequested += (_, _) => OpenStreamCommand.Execute(null);
        Home.RecentRequested += (_, source) => _ = PlayAsync(new[] { source });
    }

    private void TogglePanel(SidePanel panel) => ActivePanel = ActivePanel == panel ? SidePanel.None : panel;

    private bool IsPanelVisible(SidePanel panel) => ActivePanel == panel && !IsHomeVisible;

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
