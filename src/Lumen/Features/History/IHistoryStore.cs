namespace Lumen.Features.History;

public interface IHistoryStore
{
    event EventHandler? Changed;

    IReadOnlyList<RecentItem> GetAll();

    RecentItem? Find(string path);

    void Record(string path, TimeSpan position, TimeSpan duration);

    void Remove(string path);
}
