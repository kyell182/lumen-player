namespace Lumen.Features.Updates.Installers;

/// <summary>Weet welk release-bestand bij dit besturingssysteem hoort en hoe je het installeert.</summary>
public interface IUpdateInstaller
{
    bool IsSupported { get; }

    bool IsInstallerAsset(string assetName);

    /// <summary>Start de installatie. True = gelukt en de app moet afsluiten.</summary>
    Task<bool> InstallAsync(string installerPath);
}
