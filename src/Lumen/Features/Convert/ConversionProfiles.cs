namespace Lumen.Features.Convert;

/// <summary>De beschikbare doelformaten (vergelijkbaar met de profielen van VLC's "Converteren").</summary>
public static class ConversionProfiles
{
    private const string AudioStereo = "channels=2,samplerate=48000";

    public static readonly IReadOnlyList<ConversionProfile> All = new ConversionProfile[]
    {
        new("Video – MP4 (H.264 + AAC), werkt overal", ".mp4",
            $"#transcode{{vcodec=h264,venc=x264{{preset=faster}},acodec=mp4a,ab=192,{AudioStereo},scodec=none}}:std{{access=file,mux=mp4,dst={{dst}}}}"),
        new("Video – MKV (H.265 + AAC), kleiner bestand", ".mkv",
            $"#transcode{{vcodec=hevc,acodec=mp4a,ab=192,{AudioStereo},scodec=none}}:std{{access=file,mux=mkv,dst={{dst}}}}"),
        new("Video – WebM (VP8 + Vorbis), voor het web", ".webm",
            $"#transcode{{vcodec=VP80,vb=2500,acodec=vorb,ab=160,{AudioStereo},scodec=none}}:std{{access=file,mux=webm,dst={{dst}}}}"),
        new("Video – MP4 zonder hercoderen (snel)", ".mp4",
            "#std{access=file,mux=mp4,dst={dst}}"),
        new("Audio – MP3 (320 kb/s)", ".mp3",
            "#transcode{vcodec=none,acodec=mp3,ab=320,channels=2,samplerate=44100,scodec=none}:std{access=file,mux=raw,dst={dst}}",
            AudioOnly: true),
        new("Audio – FLAC (verliesvrij)", ".flac",
            "#transcode{vcodec=none,acodec=flac,channels=2,samplerate=44100,scodec=none}:std{access=file,mux=raw,dst={dst}}",
            AudioOnly: true),
        new("Audio – OGG Vorbis", ".ogg",
            "#transcode{vcodec=none,acodec=vorb,ab=192,channels=2,samplerate=44100,scodec=none}:std{access=file,mux=ogg,dst={dst}}",
            AudioOnly: true),
    };

    /// <summary>Een stream ongewijzigd opnemen naar een .ts-bestand.</summary>
    public static readonly ConversionProfile Recording = new("Opname", ".ts", "#std{access=file,mux=ts,dst={dst}}");
}
