using CommunityToolkit.Mvvm.ComponentModel;

namespace Lumen.Features.Effects.Audio;

/// <summary>Eén schuifregelaar van de equalizer.</summary>
public sealed partial class EqualizerBandViewModel : ObservableObject
{
    private readonly Action _onChanged;

    [ObservableProperty]
    private double _gain;

    public EqualizerBandViewModel(EqualizerBand band, Action onChanged)
    {
        Label = band.Label;
        _onChanged = onChanged;
    }

    public string Label { get; }

    /// <summary>Zet de waarde zonder de equalizer meteen opnieuw toe te passen.</summary>
    public void SetQuietly(double gain)
    {
        _gain = gain;
        OnPropertyChanged(nameof(Gain));
    }

    partial void OnGainChanged(double value) => _onChanged();
}
