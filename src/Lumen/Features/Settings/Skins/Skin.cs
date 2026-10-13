using Avalonia.Media;

namespace Lumen.Features.Settings.Skins;

/// <summary>Een kleurthema: de accentkleur en het verloop van het logo en de knoppen.</summary>
public sealed record Skin(string Name, Color Accent, Color AccentHover, Color AccentPressed, Color GradientStart, Color GradientEnd)
{
    public static Skin From(string name, string accent, string hover, string pressed, string gradientStart, string gradientEnd) =>
        new(name, Color.Parse(accent), Color.Parse(hover), Color.Parse(pressed), Color.Parse(gradientStart), Color.Parse(gradientEnd));
}
