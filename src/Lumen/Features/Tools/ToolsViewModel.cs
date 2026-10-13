using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lumen.Features.Convert;
using Lumen.Features.Playback;

namespace Lumen.Features.Tools;

/// <summary>
/// Het menu "Meer" in de bediening: schermafbeelding, volgend beeld, A-B herhalen, slaaptimer,
/// altijd bovenop en een stream opnemen.
/// </summary>
public sealed partial class ToolsViewModel : ObservableObject
{
    private readonly PlayerViewModel _player;
    private readonly SnapshotService _snapshots;
    private readonly StreamRecorder _recorder;
    private readonly Func<string?> _currentSource;

    [ObservableProperty] private bool _isAlwaysOnTop;
    [ObservableProperty] private bool _isRecording;

    public ToolsViewModel(
        PlayerViewModel player,
        SnapshotService snapshots,
        SleepTimer sleepTimer,
        StreamRecorder recorder,
        Notifier notifier,
        Func<string?> currentSource)
    {
        _player = player;
        _snapshots = snapshots;
        _recorder = recorder;
        _currentSource = currentSource;
        SleepTimer = sleepTimer;
        Notifier = notifier;

        SleepTimer.Elapsed += (_, _) =>
        {
            _player.Pause();
            Notifier.Show("Slaaptimer: gepauzeerd. Slaapwel!");
        };
    }

    public SleepTimer SleepTimer { get; }

    public Notifier Notifier { get; }

    public bool CanRecord => StreamRecorder.CanRecord(_currentSource());

    [RelayCommand]
    private void TakeSnapshot()
    {
        var path = _snapshots.Take();
        Notifier.Show(path is null
            ? "Er is geen beeld om op te slaan"
            : $"Schermafbeelding opgeslagen in Afbeeldingen\\Lumen");
    }

    [RelayCommand]
    private void StepFrame() => _player.StepFrame();

    [RelayCommand]
    private void MarkAbRepeat() => Notifier.Show(_player.MarkAbRepeat());

    [RelayCommand]
    private void ToggleAlwaysOnTop()
    {
        IsAlwaysOnTop = !IsAlwaysOnTop;
        Notifier.Show(IsAlwaysOnTop ? "Altijd bovenop: aan" : "Altijd bovenop: uit");
    }

    public void SetSleepMinutes(int minutes)
    {
        SleepTimer.StartMinutes(minutes);
        Notifier.Show($"Slaaptimer: {SleepTimer.Description.ToLowerInvariant()}");
    }

    public void SetSleepAtEndOfItem()
    {
        SleepTimer.StartAtEndOfItem();
        Notifier.Show("Slaaptimer: stopt na dit bestand");
    }

    public void CancelSleep()
    {
        SleepTimer.Cancel();
        Notifier.Show("Slaaptimer uit");
    }

    [RelayCommand]
    private void ToggleRecording()
    {
        if (IsRecording)
        {
            _recorder.Stop();
            return;
        }

        if (_currentSource() is not { } source || !StreamRecorder.CanRecord(source))
        {
            Notifier.Show("Opnemen kan alleen bij een netwerkstream");
            return;
        }

        var destination = _recorder.Start(source, OnRecordingFinished);
        IsRecording = true;
        Notifier.Show($"Opname gestart: {Path.GetFileName(destination)}");
    }

    private void OnRecordingFinished(ConversionResult result)
    {
        IsRecording = false;
        Notifier.Show(result == ConversionResult.Failed
            ? "Opname mislukt (zie vlc.log)"
            : "Opname opgeslagen in Video's\\Lumen");
    }
}
