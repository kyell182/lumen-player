using System.ComponentModel;
using System.Diagnostics;

namespace Lumen.Features.Updates.Installers;

/// <summary>
/// Start de Inno Setup-installer stil. De installer sluit Lumen, werkt bij en start Lumen daarna opnieuw.
/// </summary>
public sealed class WindowsUpdateInstaller : IUpdateInstaller
{
    private const string SilentArguments = "/SILENT /SUPPRESSMSGBOXES /NORESTART /CLOSEAPPLICATIONS";

    public bool IsSupported => true;

    public bool IsInstallerAsset(string assetName) =>
        assetName.StartsWith("Lumen-Setup-", StringComparison.OrdinalIgnoreCase)
        && assetName.EndsWith("-x64.exe", StringComparison.OrdinalIgnoreCase);

    public Task<bool> InstallAsync(string installerPath)
    {
        try
        {
            // UseShellExecute zodat Windows om administratorrechten kan vragen.
            Process.Start(new ProcessStartInfo(installerPath, SilentArguments) { UseShellExecute = true });
            return Task.FromResult(true);
        }
        catch (Win32Exception)
        {
            // Gebruiker heeft de UAC-vraag geweigerd.
            return Task.FromResult(false);
        }
    }
}
