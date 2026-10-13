using System.Collections.ObjectModel;
using Avalonia.Threading;

namespace Lumen.Features.MediaInfo;

/// <summary>Het infopaneel. Ververst elke seconde zolang het open staat (voor de statistieken).</summary>
public sealed class MediaInfoViewModel
{
    private readonly IMediaInfoSource _source;
    private readonly DispatcherTimer _refreshTimer;

    public MediaInfoViewModel(IMediaInfoSource source)
    {
        _source = source;
        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _refreshTimer.Tick += (_, _) => Refresh();
    }

    public ObservableCollection<InfoSection> Sections { get; } = new();

    public void Start()
    {
        Refresh();
        _refreshTimer.Start();
    }

    public void Stop() => _refreshTimer.Stop();

    private void Refresh()
    {
        Sections.Clear();
        foreach (var section in _source.Read()) Sections.Add(section);
    }
}
