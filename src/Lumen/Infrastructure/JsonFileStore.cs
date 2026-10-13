using System.Diagnostics;
using System.Text.Json;

namespace Lumen.Infrastructure;

/// <summary>Leest en schrijft één object als JSON-bestand. Fouten worden gelogd, nooit gegooid.</summary>
public sealed class JsonFileStore<T> where T : class, new()
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _path;

    public JsonFileStore(string path) => _path = path;

    public T Load()
    {
        try
        {
            if (!File.Exists(_path))
            {
                return new T();
            }

            using var stream = File.OpenRead(_path);
            return JsonSerializer.Deserialize<T>(stream, Options) ?? new T();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            Trace.WriteLine($"Kon {_path} niet lezen: {ex.Message}");
            return new T();
        }
    }

    public void Save(T value)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var tempPath = _path + ".tmp";
            File.WriteAllText(tempPath, JsonSerializer.Serialize(value, Options));
            File.Move(tempPath, _path, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Trace.WriteLine($"Kon {_path} niet opslaan: {ex.Message}");
        }
    }
}
