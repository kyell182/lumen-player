using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lumen.Infrastructure;

namespace Lumen.Features.Playlist;

/// <summary>De wachtrij: welke bestanden er zijn, welk bestand nu speelt en wat het volgende is.</summary>
public sealed partial class PlaylistViewModel : ObservableObject
{
    private readonly IFilePickerService _picker;

    [ObservableProperty]
    private PlaylistItemViewModel? _current;

    public PlaylistViewModel(IFilePickerService picker)
    {
        _picker = picker;
        Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(CountText));
    }

    /// <summary>Wordt gevuurd wanneer de gebruiker een item uit de lijst kiest.</summary>
    public event EventHandler<PlaylistItemViewModel>? ItemActivated;

    public ObservableCollection<PlaylistItemViewModel> Items { get; } = new();

    public string CountText => Items.Count == 1 ? "1 bestand" : $"{Items.Count} bestanden";

    public PlaylistItemViewModel? NextItem => ItemAt(CurrentIndex + 1);

    public PlaylistItemViewModel? PreviousItem => CurrentIndex > 0 ? ItemAt(CurrentIndex - 1) : null;

    private int CurrentIndex => Current is null ? -1 : Items.IndexOf(Current);

    /// <summary>Vervangt de lijst en geeft het eerste item terug.</summary>
    public PlaylistItemViewModel? Replace(IEnumerable<string> paths)
    {
        Current = null;
        Items.Clear();
        Append(paths);
        return Items.FirstOrDefault();
    }

    public void Append(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            Items.Add(new PlaylistItemViewModel(path));
        }
    }

    partial void OnCurrentChanged(PlaylistItemViewModel? oldValue, PlaylistItemViewModel? newValue)
    {
        if (oldValue is not null) oldValue.IsCurrent = false;
        if (newValue is not null) newValue.IsCurrent = true;
    }

    [RelayCommand]
    private void Play(PlaylistItemViewModel item) => ItemActivated?.Invoke(this, item);

    [RelayCommand]
    private void Remove(PlaylistItemViewModel item)
    {
        if (item == Current) return; // het spelende item blijft staan
        Items.Remove(item);
    }

    [RelayCommand]
    private void Clear()
    {
        foreach (var item in Items.Where(i => i != Current).ToList())
        {
            Items.Remove(item);
        }
    }

    [RelayCommand]
    private async Task AddFilesAsync()
    {
        var files = await _picker.PickMediaFilesAsync();
        Append(MediaFileTypes.Expand(files));
    }

    private PlaylistItemViewModel? ItemAt(int index) =>
        index >= 0 && index < Items.Count ? Items[index] : null;
}
