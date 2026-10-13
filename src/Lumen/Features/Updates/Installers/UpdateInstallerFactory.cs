namespace Lumen.Features.Updates.Installers;

public static class UpdateInstallerFactory
{
    public static IUpdateInstaller ForCurrentPlatform()
    {
        if (OperatingSystem.IsWindows()) return new WindowsUpdateInstaller();
        if (OperatingSystem.IsLinux()) return new LinuxUpdateInstaller();
        return new UnsupportedUpdateInstaller();
    }
}
