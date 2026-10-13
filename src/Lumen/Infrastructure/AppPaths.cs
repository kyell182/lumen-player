namespace Lumen.Infrastructure;

/// <summary>Locaties waar Lumen zijn gegevens bewaart (instellingen, geschiedenis).</summary>
public static class AppPaths
{
    public static string DataDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lumen");

    public static string DataFile(string fileName) => Path.Combine(DataDirectory, fileName);
}
