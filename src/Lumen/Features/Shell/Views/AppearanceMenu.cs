using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Lumen.Features.Settings;
using Lumen.Features.Settings.Skins;

namespace Lumen.Features.Shell.Views;

/// <summary>Menu "Weergave": licht/donker en de kleurthema's (skins).</summary>
public static class AppearanceMenu
{
    private const double DotSize = 14;

    public static void Show(Control anchor, ThemeService theme, SkinService skins)
    {
        var flyout = new MenuFlyout { Placement = PlacementMode.BottomEdgeAlignedRight };

        flyout.Items.Add(Header("Modus"));
        flyout.Items.Add(ThemeOption(anchor, "Donker", AppTheme.Dark, theme));
        flyout.Items.Add(ThemeOption(anchor, "Licht", AppTheme.Light, theme));
        flyout.Items.Add(new Separator());
        flyout.Items.Add(Header("Kleur"));

        foreach (var skin in SkinCatalog.All)
        {
            flyout.Items.Add(SkinOption(skin, skin == skins.Current, skins));
        }

        flyout.ShowAt(anchor);
    }

    private static MenuItem ThemeOption(Control anchor, string label, AppTheme value, ThemeService theme)
    {
        var item = new MenuItem { Header = label };
        if (theme.Current == value && anchor.FindResource("IconCheck") is Geometry check)
        {
            item.Icon = new PathIcon { Data = check, Width = 14, Height = 14 };
        }

        item.Click += (_, _) => theme.Select(value);
        return item;
    }

    private static MenuItem SkinOption(Skin skin, bool isSelected, SkinService skins)
    {
        var item = new MenuItem
        {
            Header = skin.Name,
            Icon = ColorDot(skin, isSelected),
            FontWeight = isSelected ? FontWeight.SemiBold : FontWeight.Normal,
        };
        item.Click += (_, _) => skins.Select(skin);
        return item;
    }

    private static Ellipse ColorDot(Skin skin, bool isSelected) => new()
    {
        Width = DotSize,
        Height = DotSize,
        Fill = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops = { new GradientStop(skin.GradientStart, 0), new GradientStop(skin.GradientEnd, 1) },
        },
        Stroke = isSelected ? Brushes.White : null,
        StrokeThickness = isSelected ? 2 : 0,
    };

    private static MenuItem Header(string text) => new() { Header = text, IsEnabled = false, FontSize = 11 };
}
