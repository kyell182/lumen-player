using Lumen.Infrastructure;

namespace Lumen.Features.Playlist.Files;

/// <summary>Laadt een .m3u van schijf of van internet (bv. een IPTV-lijst).</summary>
public sealed class PlaylistFileLoader
{
    private const int MaxDownloadBytes = 20 * 1024 * 1024;

    private readonly HttpClient _http;

    public PlaylistFileLoader(HttpClient http) => _http = http;

    public static bool IsPlaylist(string source)
    {
        var path = MediaSource.IsStream(source) ? new Uri(source).AbsolutePath : source;
        var extension = Path.GetExtension(path);
        return extension.Equals(".m3u", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".m3u8", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// De items in de lijst, of null als de bron geen lijst is maar een stream die je gewoon afspeelt (HLS).
    /// </summary>
    public async Task<IReadOnlyList<PlaylistEntry>?> LoadAsync(string source)
    {
        var content = MediaSource.IsStream(source)
            ? await DownloadAsync(source)
            : await File.ReadAllTextAsync(source);

        if (M3uPlaylist.IsStreamManifest(content)) return null;

        var baseFolder = MediaSource.IsStream(source) ? null : Path.GetDirectoryName(Path.GetFullPath(source));
        return M3uPlaylist.Parse(content, baseFolder);
    }

    public static Task SaveAsync(string path, IEnumerable<PlaylistEntry> entries) =>
        File.WriteAllTextAsync(path, M3uPlaylist.Serialize(entries));

    private async Task<string> DownloadAsync(string url)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength > MaxDownloadBytes)
        {
            throw new IOException("De afspeellijst is te groot.");
        }

        return await response.Content.ReadAsStringAsync(timeout.Token);
    }
}
