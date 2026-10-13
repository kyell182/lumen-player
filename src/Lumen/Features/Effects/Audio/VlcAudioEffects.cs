using LibVLCSharp.Shared;
using Lumen.Features.Playback.Engine;

namespace Lumen.Features.Effects.Audio;

/// <summary>Equalizer en vertragingen via de VLC-speler.</summary>
public sealed class VlcAudioEffects : IAudioEffects, IDisposable
{
    private const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;

    private readonly MediaPlayer _player;
    private Equalizer? _equalizer;

    public VlcAudioEffects(VlcEngine engine)
    {
        _player = engine.Player;

        using var probe = new Equalizer();
        Bands = Enumerable.Range(0, (int)probe.BandCount)
            .Select(i => new EqualizerBand(i, probe.BandFrequency((uint)i)))
            .ToList();
        PresetNames = Enumerable.Range(0, (int)probe.PresetCount)
            .Select(i => probe.PresetName((uint)i) ?? $"Voorinstelling {i + 1}")
            .ToList();
    }

    public IReadOnlyList<EqualizerBand> Bands { get; }

    public IReadOnlyList<string> PresetNames { get; }

    public EqualizerSettings PresetSettings(int presetIndex)
    {
        using var preset = new Equalizer((uint)presetIndex);
        var bands = Bands.Select(b => preset.Amp((uint)b.Index)).ToList();
        return new EqualizerSettings(preset.Preamp, bands);
    }

    public void ApplyEqualizer(EqualizerSettings? settings)
    {
        if (settings is null)
        {
            _player.UnsetEqualizer();
            return;
        }

        _equalizer ??= new Equalizer();
        _equalizer.SetPreamp(settings.Preamp);
        for (var i = 0; i < settings.Bands.Count && i < Bands.Count; i++)
        {
            _equalizer.SetAmp(settings.Bands[i], (uint)i);
        }

        _player.SetEqualizer(_equalizer);
    }

    public void SetAudioDelay(TimeSpan delay) => _player.SetAudioDelay(delay.Ticks / TicksPerMicrosecond);

    public void SetSubtitleDelay(TimeSpan delay) => _player.SetSpuDelay(delay.Ticks / TicksPerMicrosecond);

    public void Dispose() => _equalizer?.Dispose();
}
