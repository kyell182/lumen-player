namespace Lumen.Features.Settings;

public enum AppTheme
{
    Dark,
    Light,
}

public sealed class AppSettings
{
    public double Volume { get; set; } = 80;

    public AppTheme Theme { get; set; } = AppTheme.Dark;
}
