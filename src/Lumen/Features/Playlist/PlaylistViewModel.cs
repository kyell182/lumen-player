using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lumen.Features.Playlist.Files;
using Lumen.Infrastructure;

namespace Lumen.Features.Playlist;

/// <summary>De wachtrij: welke items er zijn, welk item nu speelt, herhalen, shuffle en opslaan.</summary>
public sealed partial class PlaylistViewModel : ObservableObject
{
    private readonly IFilePickerService _picker;
    private readonly PlaylistNavigator _navigator = new();

    [ObservableProperty]
    private PlaylistItemViewModel? _current;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RepeatLabel), nameof(IsRepeatOn), nameof(IsRepeatOne))]
    private RepeatMode _repeat;

    [ObservableProperty]
    private bool _isShuffle;

    public PlaylistViewModel(IFilePickerService picker)
    {
        _picker = picker;
        Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(CountText));
    }

    /// <summary>Wordt gevuurd wanneer de gebruiker een item uit de lijst kiest.</summary>
    public event EventHandler<PlaylistItemViewModel>? ItemActivated;

    public ObservableCollection<PlaylistItemViewModel> Items { get; } = new();

    public string CountText => Items.Count == 1 ? "1 item" : $"{Items.Count} items";

    public bool IsRepeatOn => Repeat != RepeatMode.Off;

    public bool IsRepeatOne => Repeat == RepeatMode.One;

    public string RepeatLabel => Repeat switch
    {
        RepeatMode.All => "Alles herhalen",
        RepeatMode.One => "Eén herhalen",
        _ => "Niet herhalen",
    };

    /// <summary>Volgende item als de gebruiker op "volgende" klikt.</summary>
    public PlaylistItemViewModel? NextItem => _navigator.Next(Items, Current, automatic: false);

    /// <summary>Volgende item als het huidige gewoon afgelopen is (houdt rekening met "één herhalen").</summary>
    public PlaylistItemViewModel? NextAfterEnd => _navigator.Next(Items, Current, automatic: true);

    public PlaylistItemViewModel? PreviousItem => _navigator.Previous(Items, Current);

    /// <summary>Vervangt de lijst en geeft het eerste item terug.</summary>
    public PlaylistItemViewModel? Replace(IEnumerable<PlaylistEntry> entries)
    {
        Current = null;
        Items.Clear();
        _navigator.IsShuffle = IsShuffle;
        Append(entries);
        return IsShuffle ? _navigator.Next(Items, null, automatic: false) : Items.FirstOrDefault();
    }

    public PlaylistItemViewModel? Replace(IEnumerable<string> sources) => Replace(sources.Select(s => new PlaylistEntry(s)));

    public void Append(IEnumerable<PlaylistEntry> entries)
    {
        foreach (var entry in entries) Items.Add(new PlaylistItemViewModel(entry));
    }

    public void Append(IEnumerable<string> sources) => Append(sources.Select(s => new PlaylistEntry(s)));

    [RelayCommand]
    private void CycleRepeat() => Repeat = Repeat switch
    {
        RepeatMode.Off => RepeatMode.All,
        RepeatMode.All => RepeatMode.One,
        _ => RepeatMode.Off,
    };

    [RelayCommand]
    private void ToggleShuffle() => IsShuffle = !IsShuffle;

    partial void OnRepeatChanged(RepeatMode value) => _navigator.Repeat = value;

    partial void OnIsShuffleChanged(bool value)
    {
        _navigator.IsShuffle = value;
        if (Current is not null) _navigator.MarkPlayed(Current);
    }

    partial void OnCurrentChanged(PlaylistItemViewModel? oldValue, PlaylistItemViewModel? newValue)
    {
        if (oldValue is not null) oldValue.IsCurrent = false;
        if (newValue is null) return;

        newValue.IsCurrent = true;
        _navigator.MarkPlayed(newValue);
    }

    [RelayCommand]
    private void Play(PlaylistItemViewModel item) => ItemActivated?.Invoke(this, item);

    [RelayCommand]
    private void Remove(PlaylistItemViewModel item)
    {
        if (item == Current) return; // het spelende item blijft staan
        Items.Remove(item);
        _navigator.Forget(item);
    }

    [RelayCommand]
    private void Clear()
    {
        foreach (var item in Items.Where(i => i != Current).ToList()) Remove(item);
    }

    [RelayCommand]
    private async Task AddFilesAsync()
    {
        var files = await _picker.PickMediaFilesAsync();
        Append(MediaFileTypes.Expand(files));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Items.Count == 0) return;

        var path = await _picker.PickSaveFileAsync("Afspeellijst opslaan", "Afspeellijst.m3u", "Afspeellijst", "m3u");
        if (path is not null) await PlaylistFileLoader.SaveAsync(path, Items.Select(i => i.Entry));
    }
}
