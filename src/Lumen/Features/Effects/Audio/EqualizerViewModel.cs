using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lumen.Features.Effects.Audio;

/// <summary>Tabblad "Geluid": de equalizer met voorinstellingen.</summary>
public sealed partial class EqualizerViewModel : ObservableObject
{
    private readonly IAudioEffects _audio;
    private bool _isLoadingPreset;

    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty] private double _preamp;
    [ObservableProperty] private string? _selectedPreset;

    public EqualizerViewModel(IAudioEffects audio)
    {
        _audio = audio;
        Presets = audio.PresetNames;
        Bands = new ObservableCollection<EqualizerBandViewModel>(audio.Bands.Select(b => new EqualizerBandViewModel(b, Apply)));
    }

    public IReadOnlyList<string> Presets { get; }

    public ObservableCollection<EqualizerBandViewModel> Bands { get; }

    public void ReapplyAll() => Apply();

    [RelayCommand]
    private void Reset()
    {
        _isLoadingPreset = true;
        Preamp = 0;
        foreach (var band in Bands) band.SetQuietly(0);
        SelectedPreset = null;
        _isLoadingPreset = false;
        Apply();
    }

    partial void OnIsEnabledChanged(bool value) => Apply();

    partial void OnPreampChanged(double value) => Apply();

    partial void OnSelectedPresetChanged(string? value)
    {
        var index = value is null ? -1 : Presets.ToList().IndexOf(value);
        if (index < 0 || _isLoadingPreset) return;

        var settings = _audio.PresetSettings(index);
        _isLoadingPreset = true;
        Preamp = settings.Preamp;
        for (var i = 0; i < Bands.Count && i < settings.Bands.Count; i++) Bands[i].SetQuietly(settings.Bands[i]);
        _isLoadingPreset = false;

        IsEnabled = true;
        Apply();
    }

    private void Apply()
    {
        if (_isLoadingPreset) return;

        _audio.ApplyEqualizer(IsEnabled
            ? new EqualizerSettings((float)Preamp, Bands.Select(b => (float)b.Gain).ToList())
            : null);
    }
}
