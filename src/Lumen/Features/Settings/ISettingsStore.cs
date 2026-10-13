namespace Lumen.Features.Settings;

public interface ISettingsStore
{
    AppSettings Current { get; }

    void Save();
}
