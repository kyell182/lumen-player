using Lumen.Infrastructure;

namespace Lumen.Features.History;

/// <summary>Bewaart de recente bestanden in history.json.</summary>
public sealed class JsonHistoryStore : IHistoryStore
{
    private const int MaxItems = 30;

    private static readonly StringComparison PathComparison =
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private readonly JsonFileStore<List<RecentItem>> _file;
    private readonly List<RecentItem> _items;

    public JsonHistoryStore(JsonFileStore<List<RecentItem>> file)
    {
        _file = file;
        _items = file.Load();
    }

    public event EventHandler? Changed;

    public IReadOnlyList<RecentItem> GetAll() =>
        _items.OrderByDescending(i => i.LastPlayed).ToList();

    public RecentItem? Find(string path) => _items.FirstOrDefault(i => SamePath(i.Path, path));

    public void Record(string path, TimeSpan position, TimeSpan duration)
    {
        _items.RemoveAll(i => SamePath(i.Path, path));
        _items.Insert(0, new RecentItem(path, position.TotalSeconds, duration.TotalSeconds, DateTimeOffset.Now));

        if (_items.Count > MaxItems)
        {
            _items.RemoveRange(MaxItems, _items.Count - MaxItems);
        }

        Persist();
    }

    public void Remove(string path)
    {
        if (_items.RemoveAll(i => SamePath(i.Path, path)) > 0)
        {
            Persist();
        }
    }

    private void Persist()
    {
        _file.Save(_items);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static bool SamePath(string a, string b) => string.Equals(a, b, PathComparison);
}
