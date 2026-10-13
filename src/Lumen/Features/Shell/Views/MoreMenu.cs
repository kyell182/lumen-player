using Avalonia.Controls;
using Avalonia.Media;
using Lumen.Features.Tools;

namespace Lumen.Features.Shell.Views;

/// <summary>Het menu "Meer" (⋮) in de bedieningsbalk.</summary>
public static class MoreMenu
{
    public static void Show(Control anchor, MainViewModel vm)
    {
        var tools = vm.Tools;
        var flyout = new MenuFlyout { Placement = PlacementMode.TopEdgeAlignedRight };

        flyout.Items.Add(Item("Schermafbeelding", "S", () => tools.TakeSnapshotCommand.Execute(null)));
        flyout.Items.Add(Item("Volgend beeld", "E", () => tools.StepFrameCommand.Execute(null)));
        flyout.Items.Add(Item("A-B herhalen", "A", () => tools.MarkAbRepeatCommand.Execute(null)));
        flyout.Items.Add(new Separator());
        flyout.Items.Add(SleepTimerItem(tools));
        flyout.Items.Add(Checked(anchor, Item("Altijd bovenop", null, () => tools.ToggleAlwaysOnTopCommand.Execute(null)), tools.IsAlwaysOnTop));

        if (tools.CanRecord || tools.IsRecording)
        {
            flyout.Items.Add(Item(tools.IsRecording ? "Opname stoppen" : "Stream opnemen", null,
                () => tools.ToggleRecordingCommand.Execute(null)));
        }

        flyout.Items.Add(new Separator());
        flyout.Items.Add(Item("Mediainfo", "I", () => vm.ToggleInfoCommand.Execute(null)));
        flyout.Items.Add(Item("Converteren…", null, () => vm.ShowConverterCommand.Execute(null)));

        flyout.ShowAt(anchor);
    }

    private static MenuItem SleepTimerItem(ToolsViewModel tools)
    {
        var timer = new MenuItem { Header = $"Slaaptimer ({tools.SleepTimer.Description.ToLowerInvariant()})" };

        foreach (var minutes in SleepTimer.MinuteChoices)
        {
            timer.Items.Add(Item($"Na {minutes} minuten", null, () => tools.SetSleepMinutes(minutes)));
        }

        timer.Items.Add(Item("Na dit bestand", null, tools.SetSleepAtEndOfItem));
        if (tools.SleepTimer.IsActive)
        {
            timer.Items.Add(new Separator());
            timer.Items.Add(Item("Uitzetten", null, tools.CancelSleep));
        }

        return timer;
    }

    private static MenuItem Item(string header, string? gesture, Action onClick)
    {
        var item = new MenuItem { Header = header, InputGesture = gesture is null ? null : Avalonia.Input.KeyGesture.Parse(gesture) };
        item.Click += (_, _) => onClick();
        return item;
    }

    private static MenuItem Checked(Control anchor, MenuItem item, bool isChecked)
    {
        if (isChecked && anchor.FindResource("IconCheck") is Geometry check)
        {
            item.Icon = new PathIcon { Data = check, Width = 14, Height = 14 };
        }

        return item;
    }
}
