using Lumen.Infrastructure;

namespace Lumen.Features.Settings;

/// <summary>Bewaart instellingen in settings.json.</summary>
public sealed class JsonSettingsStore : ISettingsStore
{
    private readonly JsonFileStore<AppSettings> _file;

    public JsonSettingsStore(JsonFileStore<AppSettings> file)
    {
        _file = file;
        Current = file.Load();
    }

    public AppSettings Current { get; }

    public void Save() => _file.Save(Current);
}
