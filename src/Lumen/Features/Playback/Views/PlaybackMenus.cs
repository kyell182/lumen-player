using Avalonia.Controls;
using Avalonia.Media;

namespace Lumen.Features.Playback.Views;

/// <summary>Bouwt de pop-upmenu's voor snelheid, audiosporen en ondertitels.</summary>
public static class PlaybackMenus
{
    public static void ShowSpeed(Control anchor, PlayerViewModel player)
    {
        var items = PlayerViewModel.PlaybackRates.Select(rate => Option(
            anchor,
            rate == 1f ? "Normaal (1×)" : TimeFormat.FormatRate(rate),
            Math.Abs(rate - player.Rate) < 0.01f,
            () => player.Rate = rate));

        Show(anchor, items);
    }

    public static void ShowAudioTracks(Control anchor, PlayerViewModel player)
    {
        var tracks = player.GetAudioTracks();
        var current = player.CurrentAudioTrack;
        IEnumerable<Control> items = tracks.Count == 0
            ? new[] { Info("Geen audiosporen") }
            : tracks.Select(t => Option(anchor, t.IsDisabledTrack ? "Uit" : t.Name, t.Id == current, () => player.SelectAudioTrack(t.Id)));

        Show(anchor, items);
    }

    public static void ShowSubtitles(Control anchor, PlayerViewModel player, Action loadFromFile)
    {
        var current = player.CurrentSubtitleTrack;
        var items = player.GetSubtitleTracks()
            .Select(t => (Control)Option(anchor, t.IsDisabledTrack ? "Uit" : t.Name, t.Id == current, () => player.SelectSubtitleTrack(t.Id)))
            .ToList();

        if (items.Count == 0) items.Add(Info("Geen ondertitels in dit bestand"));
        items.Add(new Separator());
        items.Add(Option(anchor, "Ondertitelbestand laden…", false, loadFromFile));

        Show(anchor, items);
    }

    private static MenuItem Option(Control anchor, string header, bool isSelected, Action onClick)
    {
        var item = new MenuItem { Header = header };
        if (isSelected && anchor.FindResource("IconCheck") is Geometry check)
        {
            item.Icon = new PathIcon { Data = check, Width = 14, Height = 14 };
        }

        item.Click += (_, _) => onClick();
        return item;
    }

    private static MenuItem Info(string text) => new() { Header = text, IsEnabled = false };

    private static void Show(Control anchor, IEnumerable<Control> items)
    {
        var flyout = new MenuFlyout { Placement = PlacementMode.TopEdgeAlignedRight };
        foreach (var item in items)
        {
            flyout.Items.Add(item);
        }

        flyout.ShowAt(anchor);
    }
}
