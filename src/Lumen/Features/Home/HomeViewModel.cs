using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lumen.Features.History;

namespace Lumen.Features.Home;

/// <summary>Startscherm: openen, slepen en "verder kijken".</summary>
public sealed partial class HomeViewModel : ObservableObject
{
    private const int MaxCards = 9;

    private readonly IHistoryStore _history;

    public HomeViewModel(IHistoryStore history)
    {
        _history = history;
        _history.Changed += (_, _) => Dispatcher.UIThread.Post(Refresh);
        Refresh();
    }

    public event EventHandler? OpenFilesRequested;
    public event EventHandler? OpenFolderRequested;
    public event EventHandler<string>? RecentRequested;

    public ObservableCollection<RecentItemViewModel> Recent { get; } = new();

    public bool HasRecent => Recent.Count > 0;

    public void Refresh()
    {
        Recent.Clear();
        foreach (var item in _history.GetAll().Where(i => File.Exists(i.Path)).Take(MaxCards))
        {
            Recent.Add(new RecentItemViewModel(item));
        }

        OnPropertyChanged(nameof(HasRecent));
    }

    [RelayCommand]
    private void OpenFiles() => OpenFilesRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenFolder() => OpenFolderRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void OpenRecent(RecentItemViewModel item) => RecentRequested?.Invoke(this, item.Path);

    [RelayCommand]
    private void RemoveRecent(RecentItemViewModel item) => _history.Remove(item.Path);
}
