namespace Lumen.Features.Effects.Audio;

/// <summary>Voorversterking en de versterking per frequentieband, in dB (-20 tot +20).</summary>
public sealed record EqualizerSettings(float Preamp, IReadOnlyList<float> Bands);

/// <summary>Een frequentieband van de equalizer, bv. 60 Hz of 1 kHz.</summary>
public sealed record EqualizerBand(int Index, float FrequencyHz)
{
    public string Label => FrequencyHz >= 1000 ? $"{FrequencyHz / 1000:0.#}k" : $"{FrequencyHz:0}";
}
