using System.ComponentModel;
using System.Diagnostics;

namespace Lumen.Features.Updates.Installers;

/// <summary>Installeert het .deb-pakket via pkexec (vraagt het systeemwachtwoord) en start Lumen opnieuw.</summary>
public sealed class LinuxUpdateInstaller : IUpdateInstaller
{
    public bool IsSupported => true;

    public bool IsInstallerAsset(string assetName) =>
        assetName.StartsWith("lumen_", StringComparison.Ordinal)
        && assetName.EndsWith("_amd64.deb", StringComparison.Ordinal);

    public async Task<bool> InstallAsync(string installerPath)
    {
        try
        {
            var install = new ProcessStartInfo("pkexec") { UseShellExecute = false };
            foreach (var arg in new[] { "apt-get", "install", "-y", "--allow-downgrades", installerPath })
            {
                install.ArgumentList.Add(arg);
            }

            using var process = Process.Start(install);
            if (process is null) return false;

            await process.WaitForExitAsync();
            if (process.ExitCode != 0) return false;

            Process.Start(new ProcessStartInfo("lumen") { UseShellExecute = false });
            return true;
        }
        catch (Win32Exception)
        {
            // pkexec of lumen niet gevonden.
            return false;
        }
    }
}
