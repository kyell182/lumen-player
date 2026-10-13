using System.Text;
using Lumen.Infrastructure;

namespace Lumen.Features.Playlist.Files;

/// <summary>Leest en schrijft .m3u-afspeellijsten (ook IPTV-lijsten met #EXTINF-titels).</summary>
public static class M3uPlaylist
{
    private const string Header = "#EXTM3U";
    private const string InfoPrefix = "#EXTINF:";

    /// <summary>True voor een HLS-stream (.m3u8 met videosegmenten): die speel je af, niet uitpakken.</summary>
    public static bool IsStreamManifest(string content) => content.Contains("#EXT-X-", StringComparison.Ordinal);

    public static IReadOnlyList<PlaylistEntry> Parse(string content, string? baseFolder)
    {
        var entries = new List<PlaylistEntry>();
        string? pendingTitle = null;

        foreach (var rawLine in content.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;

            if (line.StartsWith(InfoPrefix, StringComparison.OrdinalIgnoreCase))
            {
                pendingTitle = TitleFromInfo(line);
            }
            else if (!line.StartsWith('#'))
            {
                entries.Add(new PlaylistEntry(ResolveSource(line, baseFolder), pendingTitle));
                pendingTitle = null;
            }
        }

        return entries;
    }

    public static string Serialize(IEnumerable<PlaylistEntry> entries)
    {
        var text = new StringBuilder().AppendLine(Header);
        foreach (var entry in entries)
        {
            text.Append(InfoPrefix).Append("-1,").AppendLine(entry.DisplayTitle);
            text.AppendLine(entry.Source);
        }

        return text.ToString();
    }

    // "#EXTINF:-1 tvg-id="x" group-title="Nieuws",VRT 1" → "VRT 1"
    private static string? TitleFromInfo(string line)
    {
        var comma = line.LastIndexOf(',');
        var title = comma >= 0 ? line[(comma + 1)..].Trim() : string.Empty;
        return title.Length > 0 ? title : null;
    }

    private static string ResolveSource(string line, string? baseFolder)
    {
        if (MediaSource.IsStream(line)) return line;
        if (Uri.TryCreate(line, UriKind.Absolute, out var uri) && uri.IsFile) return uri.LocalPath;
        if (Path.IsPathRooted(line) || baseFolder is null) return line;
        return Path.GetFullPath(Path.Combine(baseFolder, line));
    }
}
