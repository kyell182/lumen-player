using System.Globalization;
using System.Text;
using LibVLCSharp.Shared;
using Lumen.Features.Playback;
using Lumen.Features.Playback.Engine;
using Lumen.Features.Playback.Tracks;
using Lumen.Infrastructure;

namespace Lumen.Features.MediaInfo;

/// <summary>Leest codecs, resolutie, kanalen en statistieken uit VLC.</summary>
public sealed class VlcMediaInfoSource : IMediaInfoSource
{
    private static readonly CultureInfo Dutch = CultureInfo.GetCultureInfo("nl-BE");

    private readonly MediaPlayer _player;

    public VlcMediaInfoSource(VlcEngine engine) => _player = engine.Player;

    public IReadOnlyList<InfoSection> Read()
    {
        using var media = _player.Media;
        if (media is null) return Array.Empty<InfoSection>();

        var sections = new List<InfoSection> { General(media) };
        foreach (var track in media.Tracks)
        {
            if (Describe(media, track) is { } section) sections.Add(section);
        }

        sections.Add(Statistics(media.Statistics));
        return sections;
    }

    private static InfoSection General(Media media)
    {
        var source = Uri.UnescapeDataString(media.Mrl);
        var rows = new List<InfoRow>
        {
            new("Bron", source.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ? new Uri(source).LocalPath : source),
            new("Duur", TimeFormat.Format(TimeSpan.FromMilliseconds(Math.Max(0, media.Duration)))),
        };

        if (media.Mrl.StartsWith("file:", StringComparison.OrdinalIgnoreCase) && new Uri(media.Mrl).LocalPath is var path && File.Exists(path))
        {
            rows.Add(new InfoRow("Bestandsgrootte", FormatBytes(new FileInfo(path).Length)));
        }

        return new InfoSection("Algemeen", rows);
    }

    private static InfoSection? Describe(Media media, MediaTrack track)
    {
        var rows = new List<InfoRow> { new("Codec", CodecName(media, track)) };

        switch (track.TrackType)
        {
            case TrackType.Video:
                var video = track.Data.Video;
                rows.Add(new InfoRow("Resolutie", $"{video.Width} × {video.Height}"));
                if (video.FrameRateDen > 0)
                {
                    rows.Add(new InfoRow("Beeldfrequentie", $"{(double)video.FrameRateNum / video.FrameRateDen:0.###} fps".Replace('.', ',')));
                }
                break;
            case TrackType.Audio:
                var audio = track.Data.Audio;
                rows.Add(new InfoRow("Kanalen", ChannelName(audio.Channels)));
                rows.Add(new InfoRow("Samplefrequentie", $"{audio.Rate.ToString("N0", Dutch)} Hz"));
                break;
            case TrackType.Text:
                break;
            default:
                return null;
        }

        if (LanguageNames.ToDutch(track.Language) is { } language) rows.Add(new InfoRow("Taal", language));
        if (!string.IsNullOrWhiteSpace(track.Description)) rows.Add(new InfoRow("Naam", track.Description!));
        if (track.Bitrate > 0) rows.Add(new InfoRow("Bitrate", $"{track.Bitrate / 1000:N0} kb/s"));

        var title = track.TrackType switch
        {
            TrackType.Video => "Video",
            TrackType.Audio => "Audio",
            _ => "Ondertitels",
        };
        return new InfoSection($"{title} (spoor {track.Id})", rows);
    }

    private static InfoSection Statistics(MediaStats stats) => new("Statistieken", new[]
    {
        new InfoRow("Invoerbitrate", $"{stats.InputBitrate * 8000:N0} kb/s"),
        new InfoRow("Getoonde beelden", stats.DisplayedPictures.ToString("N0", Dutch)),
        new InfoRow("Verloren beelden", stats.LostPictures.ToString("N0", Dutch)),
    });

    private static string CodecName(Media media, MediaTrack track)
    {
        var description = media.CodecDescription(track.TrackType, track.Codec);
        var fourcc = FourCc(track.OriginalFourcc != 0 ? track.OriginalFourcc : track.Codec);
        return string.IsNullOrWhiteSpace(description) ? fourcc : $"{description} ({fourcc})";
    }

    private static string FourCc(uint code)
    {
        var bytes = BitConverter.GetBytes(code);
        return Encoding.ASCII.GetString(bytes).Trim('\0', ' ');
    }

    private static string ChannelName(uint channels) => channels switch
    {
        1 => "Mono",
        2 => "Stereo",
        6 => "5.1 surround",
        8 => "7.1 surround",
        _ => $"{channels} kanalen",
    };

    private static string FormatBytes(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double value = bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value.ToString("0.##", Dutch)} {units[unit]}";
    }
}
