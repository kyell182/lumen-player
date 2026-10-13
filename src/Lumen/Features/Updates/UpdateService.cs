using Lumen.Features.Updates.Installers;

namespace Lumen.Features.Updates;

/// <summary>Zoekt een nieuwere versie en installeert die.</summary>
public sealed class UpdateService
{
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(20);

    private readonly IReleaseSource _source;
    private readonly UpdateDownloader _downloader;
    private readonly IUpdateInstaller _installer;

    public UpdateService(IReleaseSource source, UpdateDownloader downloader, IUpdateInstaller installer)
    {
        _source = source;
        _downloader = downloader;
        _installer = installer;
    }

    public async Task<UpdateInfo?> FindUpdateAsync()
    {
        if (!_installer.IsSupported) return null;

        using var timeout = new CancellationTokenSource(CheckTimeout);
        var release = await _source.GetLatestAsync(timeout.Token);
        if (release is null || release.Version <= AppVersion.Current) return null;

        var installer = release.Assets.FirstOrDefault(a => _installer.IsInstallerAsset(a.Name));
        return installer is null ? null : new UpdateInfo(release.Version, release.PageUrl, installer);
    }

    public Task<string> DownloadAsync(UpdateInfo update, IProgress<double> progress) =>
        _downloader.DownloadAsync(update.Installer, progress, CancellationToken.None);

    public Task<bool> InstallAsync(string installerPath) => _installer.InstallAsync(installerPath);
}
