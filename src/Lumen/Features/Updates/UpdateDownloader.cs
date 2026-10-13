namespace Lumen.Features.Updates;

/// <summary>Downloadt een installer naar een tijdelijke map en meldt de voortgang (0–100).</summary>
public sealed class UpdateDownloader
{
    private const int BufferSize = 81920;

    private readonly HttpClient _http;

    public UpdateDownloader(HttpClient http) => _http = http;

    public async Task<string> DownloadAsync(ReleaseAsset asset, IProgress<double> progress, CancellationToken cancellationToken)
    {
        var folder = Path.Combine(Path.GetTempPath(), "Lumen-update");
        Directory.CreateDirectory(folder);
        var target = Path.Combine(folder, asset.Name);

        using var response = await _http.GetAsync(asset.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        var total = response.Content.Headers.ContentLength;

        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var destination = File.Create(target);
        await CopyWithProgressAsync(source, destination, total, progress, cancellationToken);

        return target;
    }

    private static async Task CopyWithProgressAsync(
        Stream source, Stream destination, long? total, IProgress<double> progress, CancellationToken cancellationToken)
    {
        var buffer = new byte[BufferSize];
        long written = 0;
        int read;
        while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            written += read;
            if (total is > 0) progress.Report(written * 100d / total.Value);
        }

        progress.Report(100);
    }
}
