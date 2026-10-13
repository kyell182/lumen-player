using System.Text;

namespace Lumen.Features.Playback.Subtitles.Parsing;

/// <summary>
/// Leest een ondertitelbestand als tekst. Probeert UTF-8 en valt terug op Windows-1252,
/// de codering van veel oudere Nederlandse en Engelse .srt-bestanden.
/// </summary>
public static class SubtitleFileReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    static SubtitleFileReader() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    public static string Read(string path)
    {
        var bytes = File.ReadAllBytes(path);

        if (HasBom(bytes, out var bomEncoding, out var bomLength))
        {
            return bomEncoding.GetString(bytes, bomLength, bytes.Length - bomLength);
        }

        try
        {
            return StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.GetEncoding(1252).GetString(bytes);
        }
    }

    private static bool HasBom(byte[] bytes, out Encoding encoding, out int length)
    {
        (encoding, length) = bytes switch
        {
            [0xEF, 0xBB, 0xBF, ..] => (Encoding.UTF8, 3),
            [0xFF, 0xFE, ..] => (Encoding.Unicode, 2),
            [0xFE, 0xFF, ..] => (Encoding.BigEndianUnicode, 2),
            _ => ((Encoding)Encoding.UTF8, 0),
        };
        return length > 0;
    }
}
