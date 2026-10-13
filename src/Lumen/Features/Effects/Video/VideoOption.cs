namespace Lumen.Features.Effects.Video;

/// <summary>Een keuze in een lijst (bv. beeldverhouding "16:9"), met de waarde die VLC verwacht.</summary>
public sealed record VideoOption<T>(string Label, T Value)
{
    public override string ToString() => Label;
}

/// <summary>De keuzelijsten voor beeldverhouding, bijsnijden, zoom en draaien.</summary>
public static class VideoOptions
{
    public static readonly IReadOnlyList<VideoOption<string?>> AspectRatios = new VideoOption<string?>[]
    {
        new("Automatisch", null),
        new("16:9", "16:9"),
        new("4:3", "4:3"),
        new("21:9", "21:9"),
        new("2.35:1", "235:100"),
        new("16:10", "16:10"),
        new("5:4", "5:4"),
        new("1:1", "1:1"),
    };

    public static readonly IReadOnlyList<VideoOption<string?>> Crops = new VideoOption<string?>[]
    {
        new("Niet bijsnijden", null),
        new("16:9", "16:9"),
        new("4:3", "4:3"),
        new("21:9", "21:9"),
        new("2.35:1", "235:100"),
        new("1.85:1", "185:100"),
    };

    public static readonly IReadOnlyList<VideoOption<float>> Zooms = new VideoOption<float>[]
    {
        new("Passend", 0f),
        new("25%", 0.25f),
        new("50%", 0.5f),
        new("100% (origineel)", 1f),
        new("200%", 2f),
    };

    public static readonly IReadOnlyList<VideoOption<string?>> Rotations = new VideoOption<string?>[]
    {
        new("Niet draaien", null),
        new("90° rechtsom", "90"),
        new("180°", "180"),
        new("90° linksom", "270"),
        new("Horizontaal spiegelen", "hflip"),
        new("Verticaal spiegelen", "vflip"),
    };
}
