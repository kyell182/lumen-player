using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Lumen.Features.Shell;

/// <summary>
/// Doorzichtig venster dat precies over het videovlak ligt. Nodig omdat VLC in een native venster tekent,
/// waar Avalonia niets overheen kan tekenen. Volgt positie, grootte en zichtbaarheid van het videovlak.
/// </summary>
public sealed class OverlayWindow : Window
{
    private readonly Window _owner;
    private readonly Control _target;

    public OverlayWindow(Window owner, Control target, Control content)
    {
        _owner = owner;
        _target = target;

        SystemDecorations = SystemDecorations.None;
        ShowInTaskbar = false;
        ShowActivated = false;
        CanResize = false;
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
        Background = Brushes.Transparent;
        Content = content;

        WireFollowing();
    }

    private void WireFollowing()
    {
        _owner.PositionChanged += (_, _) => Sync();
        _owner.PropertyChanged += (_, e) =>
        {
            if (e.Property == WindowStateProperty || e.Property == IsVisibleProperty) Sync();
        };
        _target.LayoutUpdated += (_, _) => Sync();
        _target.PropertyChanged += (_, e) =>
        {
            if (e.Property == IsVisibleProperty) Sync();
        };
        _owner.Closing += (_, _) => Close();
    }

    /// <summary>Zet het overlay-venster op de plek van het videovlak, of verbergt het.</summary>
    public void Sync()
    {
        if (!ShouldBeVisible())
        {
            if (IsVisible) Hide();
            return;
        }

        var topLeft = _target.PointToScreen(new Point(0, 0));
        if (Position != topLeft) Position = topLeft;

        var size = _target.Bounds.Size;
        if (Math.Abs(Width - size.Width) > 0.5) Width = size.Width;
        if (Math.Abs(Height - size.Height) > 0.5) Height = size.Height;

        if (!IsVisible) ShowAboveOwner();
    }

    private void ShowAboveOwner()
    {
        try
        {
            Show(_owner);
        }
        catch (InvalidOperationException)
        {
            // Sommige versies laten een tweede Show(owner) niet toe; het eigenaarschap blijft dan bestaan.
            Show();
        }
    }

    private bool ShouldBeVisible() =>
        _owner.IsVisible
        && _owner.WindowState != WindowState.Minimized
        && _target.IsEffectivelyVisible
        && _target.Bounds.Width > 1
        && _target.Bounds.Height > 1;
}
