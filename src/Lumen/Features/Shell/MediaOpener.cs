using Lumen.Features.Playlist;
using Lumen.Features.Playlist.Files;
using Lumen.Infrastructure;

namespace Lumen.Features.Shell;

/// <summary>
/// Zet wat de gebruiker opent (bestanden, mappen, .m3u-lijsten, netwerkadressen) om naar items voor de afspeellijst.
/// </summary>
public sealed class MediaOpener
{
    private readonly PlaylistFileLoader _playlists;

    public MediaOpener(PlaylistFileLoader playlists) => _playlists = playlists;

    public async Task<IReadOnlyList<PlaylistEntry>> ToEntriesAsync(IEnumerable<string> inputs)
    {
        var entries = new List<PlaylistEntry>();
        foreach (var input in inputs)
        {
            if (await TryLoadPlaylistAsync(input) is { } listed)
            {
                entries.AddRange(listed);
                continue;
            }

            entries.AddRange(MediaFileTypes.Expand(new[] { input }).Select(source => new PlaylistEntry(source)));
        }

        return entries;
    }

    // Null = geen afspeellijst (of een HLS-stream die je gewoon afspeelt).
    private async Task<IReadOnlyList<PlaylistEntry>?> TryLoadPlaylistAsync(string input)
    {
        if (!PlaylistFileLoader.IsPlaylist(input) || !MediaSource.IsAvailable(input)) return null;

        try
        {
            return await _playlists.LoadAsync(input);
        }
        catch (Exception ex) when (ex is IOException or HttpRequestException or TaskCanceledException or UnauthorizedAccessException)
        {
            CrashLogger.Write($"Afspeellijst {input} kon niet geladen worden", ex);
            return null;
        }
    }
}
