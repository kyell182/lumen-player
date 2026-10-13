using Avalonia.Controls;
using Avalonia.Input;

namespace Lumen.Features.Shell.Views;

/// <summary>Het menu "Openen" in de titelbalk.</summary>
public static class OpenMenu
{
    public static void Show(Control anchor, MainViewModel vm)
    {
        var flyout = new MenuFlyout { Placement = PlacementMode.BottomEdgeAlignedRight };

        flyout.Items.Add(Item("Bestanden…", "Ctrl+O", () => vm.OpenFilesCommand.Execute(null)));
        flyout.Items.Add(Item("Map…", null, () => vm.OpenFolderCommand.Execute(null)));
        flyout.Items.Add(Item("Netwerkstream…", "Ctrl+N", () => vm.OpenStreamCommand.Execute(null)));
        flyout.Items.Add(Item("Afspeellijst (.m3u)…", "Ctrl+L", () => vm.OpenPlaylistCommand.Execute(null)));
        flyout.Items.Add(new Separator());
        flyout.Items.Add(Item("Converteren…", null, () => vm.ShowConverterCommand.Execute(null)));

        flyout.ShowAt(anchor);
    }

    private static MenuItem Item(string header, string? gesture, Action onClick)
    {
        var item = new MenuItem { Header = header, InputGesture = gesture is null ? null : KeyGesture.Parse(gesture) };
        item.Click += (_, _) => onClick();
        return item;
    }
}
