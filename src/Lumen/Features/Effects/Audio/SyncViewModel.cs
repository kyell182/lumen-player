using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lumen.Features.Playback.Subtitles;

namespace Lumen.Features.Effects.Audio;

/// <summary>Tabblad "Synchronisatie": geluid en ondertitels vroeger of later zetten.</summary>
public sealed partial class SyncViewModel : ObservableObject
{
    public const double StepMilliseconds = 50;

    private readonly IAudioEffects _audio;
    private readonly SubtitlesViewModel _subtitles;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AudioDelayText))]
    private double _audioDelayMs;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubtitleDelayText))]
    private double _subtitleDelayMs;

    public SyncViewModel(IAudioEffects audio, SubtitlesViewModel subtitles)
    {
        _audio = audio;
        _subtitles = subtitles;
    }

    public string AudioDelayText => Describe(AudioDelayMs);

    public string SubtitleDelayText => Describe(SubtitleDelayMs);

    /// <summary>Vertragingen gelden per bestand in VLC, dus na het openen opnieuw zetten.</summary>
    public void ReapplyAll()
    {
        OnAudioDelayMsChanged(AudioDelayMs);
        OnSubtitleDelayMsChanged(SubtitleDelayMs);
    }

    public void NudgeSubtitles(int direction) => SubtitleDelayMs += direction * StepMilliseconds;

    [RelayCommand]
    private void ResetAudio() => AudioDelayMs = 0;

    [RelayCommand]
    private void ResetSubtitles() => SubtitleDelayMs = 0;

    partial void OnAudioDelayMsChanged(double value) => _audio.SetAudioDelay(TimeSpan.FromMilliseconds(value));

    partial void OnSubtitleDelayMsChanged(double value)
    {
        var delay = TimeSpan.FromMilliseconds(value);
        _audio.SetSubtitleDelay(delay);
        _subtitles.Delay = delay;
    }

    private static string Describe(double milliseconds)
    {
        if (Math.Abs(milliseconds) < 1) return "Geen vertraging";
        var seconds = (milliseconds / 1000).ToString("+0.00;-0.00", CultureInfo.GetCultureInfo("nl-BE"));
        return milliseconds > 0 ? $"{seconds} s (later)" : $"{seconds} s (vroeger)";
    }
}
