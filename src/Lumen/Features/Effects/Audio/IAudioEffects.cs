namespace Lumen.Features.Effects.Audio;

public interface IAudioEffects
{
    IReadOnlyList<EqualizerBand> Bands { get; }

    /// <summary>Namen van de ingebouwde voorinstellingen ("Rock", "Pop", …).</summary>
    IReadOnlyList<string> PresetNames { get; }

    EqualizerSettings PresetSettings(int presetIndex);

    /// <summary>Zet de equalizer aan met deze waarden, of uit met null.</summary>
    void ApplyEqualizer(EqualizerSettings? settings);

    /// <summary>Positief = geluid later.</summary>
    void SetAudioDelay(TimeSpan delay);

    /// <summary>Positief = ondertitels later (voor ondertitels die VLC tekent).</summary>
    void SetSubtitleDelay(TimeSpan delay);
}
