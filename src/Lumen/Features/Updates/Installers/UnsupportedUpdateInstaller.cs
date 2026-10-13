namespace Lumen.Features.Updates.Installers;

/// <summary>Voor platformen zonder installer: er worden dan nooit updates aangeboden.</summary>
public sealed class UnsupportedUpdateInstaller : IUpdateInstaller
{
    public bool IsSupported => false;

    public bool IsInstallerAsset(string assetName) => false;

    public Task<bool> InstallAsync(string installerPath) => Task.FromResult(false);
}
