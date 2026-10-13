using Avalonia.Controls;
using Avalonia.Media;

namespace Lumen.Features.Cast.Views;

/// <summary>Het menu onder de castknop: gevonden apparaten en "stoppen met casten".</summary>
public static class CastMenu
{
    public static void Show(Control anchor, CastViewModel cast)
    {
        cast.EnsureDiscovery();
        var flyout = new MenuFlyout { Placement = PlacementMode.TopEdgeAlignedRight };

        flyout.Items.Add(new MenuItem { Header = "Casten naar…", IsEnabled = false, FontSize = 11 });

        if (cast.Devices.Count == 0)
        {
            flyout.Items.Add(new MenuItem
            {
                Header = "Zoeken naar Chromecasts op je netwerk… (open dit menu zo opnieuw)",
                IsEnabled = false,
            });
        }

        foreach (var device in cast.Devices)
        {
            var item = new MenuItem { Header = device.Name };
            if (device == cast.Current && anchor.FindResource("IconCheck") is Geometry check)
            {
                item.Icon = new PathIcon { Data = check, Width = 14, Height = 14 };
            }

            item.Click += (_, _) => cast.CastTo(device);
            flyout.Items.Add(item);
        }

        if (cast.IsCasting)
        {
            flyout.Items.Add(new Separator());
            var stop = new MenuItem { Header = "Stoppen met casten" };
            stop.Click += (_, _) => cast.CastTo(null);
            flyout.Items.Add(stop);
        }

        flyout.ShowAt(anchor);
    }
}
