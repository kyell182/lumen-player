using Avalonia;
using Avalonia.Styling;

namespace Lumen.Features.Settings;

/// <summary>Schakelt tussen licht en donker thema en onthoudt de keuze.</summary>
public sealed class ThemeService
{
    private readonly ISettingsStore _settings;

    public ThemeService(ISettingsStore settings) => _settings = settings;

    public AppTheme Current => _settings.Current.Theme;

    public void ApplySaved() => Apply(Current);

    public void Select(AppTheme theme)
    {
        _settings.Current.Theme = theme;
        _settings.Save();
        Apply(theme);
    }

    private static void Apply(AppTheme theme)
    {
        if (Application.Current is { } app)
        {
            app.RequestedThemeVariant = theme == AppTheme.Light ? ThemeVariant.Light : ThemeVariant.Dark;
        }
    }
}
