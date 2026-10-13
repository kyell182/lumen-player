namespace Lumen.Features.Playlist;

/// <summary>Bepaalt wat er na (of vóór) het huidige item komt, rekening houdend met herhalen en shuffle.</summary>
public sealed class PlaylistNavigator
{
    private readonly Random _random = new();
    private readonly List<PlaylistItemViewModel> _shuffleHistory = new();
    private bool _isShuffle;

    public RepeatMode Repeat { get; set; }

    public bool IsShuffle
    {
        get => _isShuffle;
        set
        {
            _isShuffle = value;
            _shuffleHistory.Clear();
        }
    }

    /// <summary>Onthoudt wat gespeeld is, zodat shuffle niets herhaalt en "vorige" werkt.</summary>
    public void MarkPlayed(PlaylistItemViewModel item)
    {
        if (IsShuffle && _shuffleHistory.LastOrDefault() != item) _shuffleHistory.Add(item);
    }

    public void Forget(PlaylistItemViewModel item) => _shuffleHistory.RemoveAll(i => i == item);

    /// <param name="automatic">True als het vorige item gewoon afgelopen is (dan telt "herhaal één").</param>
    public PlaylistItemViewModel? Next(IReadOnlyList<PlaylistItemViewModel> items, PlaylistItemViewModel? current, bool automatic)
    {
        if (items.Count == 0) return null;
        if (automatic && Repeat == RepeatMode.One && current is not null) return current;
        if (IsShuffle) return NextShuffled(items, current);

        var index = current is null ? -1 : IndexOf(items, current);
        if (index + 1 < items.Count) return items[index + 1];
        return Repeat == RepeatMode.All ? items[0] : null;
    }

    public PlaylistItemViewModel? Previous(IReadOnlyList<PlaylistItemViewModel> items, PlaylistItemViewModel? current)
    {
        if (items.Count == 0) return null;

        if (IsShuffle && _shuffleHistory.Count > 1)
        {
            _shuffleHistory.RemoveAt(_shuffleHistory.Count - 1);
            return _shuffleHistory[^1];
        }

        var index = current is null ? 0 : IndexOf(items, current);
        if (index > 0) return items[index - 1];
        return Repeat == RepeatMode.All ? items[^1] : null;
    }

    private PlaylistItemViewModel? NextShuffled(IReadOnlyList<PlaylistItemViewModel> items, PlaylistItemViewModel? current)
    {
        var candidates = items.Where(i => i != current && !_shuffleHistory.Contains(i)).ToList();
        if (candidates.Count == 0)
        {
            if (Repeat != RepeatMode.All) return null;
            _shuffleHistory.Clear();
            candidates = items.Where(i => i != current).ToList();
            if (candidates.Count == 0) return current;
        }

        return candidates[_random.Next(candidates.Count)];
    }

    private static int IndexOf(IReadOnlyList<PlaylistItemViewModel> items, PlaylistItemViewModel item)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i] == item) return i;
        }

        return -1;
    }
}
