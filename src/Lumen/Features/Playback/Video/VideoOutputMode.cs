namespace Lumen.Features.Playback.Video;

public enum VideoOutputMode
{
    /// <summary>VLC tekent zelf via de GPU in een venster van Lumen (snelst, alle ondertitels). Standaard op Windows.</summary>
    NativeWindow,

    /// <summary>VLC levert frames aan Lumen, die ze zelf tekent. Standaard op Linux.</summary>
    Frames,
}

public static class VideoOutputModes
{
    public static VideoOutputMode ForCurrentPlatform() =>
        OperatingSystem.IsWindows() ? VideoOutputMode.NativeWindow : VideoOutputMode.Frames;
}
