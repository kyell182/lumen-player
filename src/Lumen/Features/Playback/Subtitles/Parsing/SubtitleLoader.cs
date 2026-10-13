namespace Lumen.Features.Playback.Subtitles.Parsing;

/// <summary>Laadt een ondertitelbestand dat Lumen zelf kan tonen (.srt, .vtt, .ass, .ssa).</summary>
public static class SubtitleLoader
{
    private static readonly Dictionary<string, ISubtitleParser> Parsers = new(StringComparer.OrdinalIgnoreCase)
    {
        [".srt"] = new SrtParser(),
        [".vtt"] = new SrtParser(),
        [".ass"] = new AssParser(),
        [".ssa"] = new AssParser(),
    };

    public static bool CanLoad(string path) => Parsers.ContainsKey(Path.GetExtension(path));

    /// <summary>Het document, of null als het bestand onleesbaar of leeg is.</summary>
    public static SubtitleDocument? TryLoad(string path)
    {
        if (!Parsers.TryGetValue(Path.GetExtension(path), out var parser)) return null;

        try
        {
            var document = new SubtitleDocument(parser.Parse(SubtitleFileReader.Read(path)));
            return document.Count > 0 ? document : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
