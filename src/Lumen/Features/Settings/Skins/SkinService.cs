using Avalonia;
using Avalonia.Media;

namespace Lumen.Features.Settings.Skins;

/// <summary>
/// Past een skin toe door de accent-resources van de app te vervangen. Alles wat die resources
/// dynamisch gebruikt (knoppen, sliders, voortgangsbalken, logo) kleurt meteen mee.
/// </summary>
public sealed class SkinService
{
    private readonly ISettingsStore _settings;

    public SkinService(ISettingsStore settings) => _settings = settings;

    public Skin Current => SkinCatalog.ByName(_settings.Current.Skin);

    public void ApplySaved() => Apply(Current);

    public void Select(Skin skin)
    {
        _settings.Current.Skin = skin.Name;
        _settings.Save();
        Apply(skin);
    }

    private static void Apply(Skin skin)
    {
        if (Application.Current is not { } app) return;

        var resources = app.Resources;
        resources["AccentColor"] = skin.Accent;
        resources["AccentBrush"] = new SolidColorBrush(skin.Accent);
        resources["AccentHoverBrush"] = new SolidColorBrush(skin.AccentHover);
        resources["AccentPressedBrush"] = new SolidColorBrush(skin.AccentPressed);
        resources["BrandGradientBrush"] = Gradient(skin);

        // Fluent-sliders gebruiken deze sleutels; zo kleuren tijdlijn en volume mee.
        resources["SliderTrackValueFill"] = new SolidColorBrush(skin.Accent);
        resources["SliderTrackValueFillPointerOver"] = new SolidColorBrush(skin.AccentHover);
        resources["SliderTrackValueFillPressed"] = new SolidColorBrush(skin.AccentPressed);
        resources["SliderThumbBackground"] = new SolidColorBrush(skin.Accent);
        resources["SliderThumbBackgroundPointerOver"] = new SolidColorBrush(skin.AccentHover);
        resources["SliderThumbBackgroundPressed"] = new SolidColorBrush(skin.AccentPressed);

        resources["SystemAccentColor"] = skin.Accent;
        resources["SystemAccentColorLight1"] = skin.AccentHover;
        resources["SystemAccentColorDark1"] = skin.AccentPressed;
    }

    private static LinearGradientBrush Gradient(Skin skin) => new()
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops =
        {
            new GradientStop(skin.GradientStart, 0),
            new GradientStop(skin.GradientEnd, 1),
        },
    };
}
