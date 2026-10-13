namespace Lumen.Features.Updates;

/// <summary>De versie van de draaiende app, genormaliseerd naar major.minor.build.</summary>
public static class AppVersion
{
    public static Version Current { get; } = Normalize(typeof(AppVersion).Assembly.GetName().Version);

    public static bool TryParseTag(string tag, out Version version)
    {
        if (Version.TryParse(tag.TrimStart('v', 'V'), out var parsed))
        {
            version = Normalize(parsed);
            return true;
        }

        version = new Version(0, 0, 0);
        return false;
    }

    // 1.0.3 en 1.0.3.0 moeten als gelijk gelden.
    private static Version Normalize(Version? version) => version is null
        ? new Version(0, 0, 0)
        : new Version(version.Major, Math.Max(0, version.Minor), Math.Max(0, version.Build));
}
