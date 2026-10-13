namespace Lumen.Features.Updates;

public sealed record ReleaseAsset(string Name, string DownloadUrl);

/// <summary>Een gepubliceerde release met de bijhorende downloadbestanden.</summary>
public sealed record ReleaseInfo(Version Version, string PageUrl, IReadOnlyList<ReleaseAsset> Assets);

/// <summary>Een release die nieuwer is dan de geïnstalleerde versie, met het installer-bestand voor dit platform.</summary>
public sealed record UpdateInfo(Version Version, string PageUrl, ReleaseAsset Installer);
