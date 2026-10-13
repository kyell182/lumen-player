using LibVLCSharp.Shared;
using Lumen.Features.Playback.Engine;
using Lumen.Infrastructure;

namespace Lumen.Features.Network;

/// <summary>
/// Sommige webadressen (bv. videosites) zijn geen video maar een pagina. VLC kan daar vaak de echte
/// videostream uit halen; dit zet zo'n adres om naar het adres dat effectief afgespeeld wordt.
/// </summary>
public sealed class StreamResolver
{
    private static readonly TimeSpan ParseTimeout = TimeSpan.FromSeconds(10);

    private readonly LibVLC _libVlc;

    public StreamResolver(VlcEngine engine) => _libVlc = engine.LibVlc;

    public async Task<string> ResolveAsync(string source)
    {
        if (!NeedsResolving(source)) return source;

        using var media = new Media(_libVlc, new Uri(source));
        var status = await media.Parse(MediaParseOptions.ParseNetwork, (int)ParseTimeout.TotalMilliseconds);
        if (status != MediaParsedStatus.Done) return source;

        using var subItems = media.SubItems;
        if (subItems.Count == 0) return source;

        using var first = subItems[0];
        return first?.Mrl ?? source;
    }

    // Alleen webpagina-adressen; rechtstreekse bestanden en rtsp/udp-streams kan VLC meteen afspelen.
    private static bool NeedsResolving(string source)
    {
        if (!MediaSource.IsStream(source)) return false;

        var uri = new Uri(source);
        if (uri.Scheme is not ("http" or "https")) return false;

        return !MediaFileTypes.IsPlayable(uri.AbsolutePath) && !uri.AbsolutePath.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase);
    }
}
