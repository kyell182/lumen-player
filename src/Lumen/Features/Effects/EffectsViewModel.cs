using Lumen.Features.Effects.Audio;
using Lumen.Features.Effects.Video;

namespace Lumen.Features.Effects;

/// <summary>Het paneel "Aanpassingen" met de tabbladen Beeld, Geluid en Synchronisatie.</summary>
public sealed class EffectsViewModel
{
    public EffectsViewModel(VideoEffectsViewModel video, EqualizerViewModel equalizer, SyncViewModel sync)
    {
        Video = video;
        Equalizer = equalizer;
        Sync = sync;
    }

    public VideoEffectsViewModel Video { get; }

    public EqualizerViewModel Equalizer { get; }

    public SyncViewModel Sync { get; }

    /// <summary>Sommige instellingen gelden per bestand: na elk nieuw bestand opnieuw toepassen.</summary>
    public void ReapplyAfterMediaChange()
    {
        Video.ReapplyAll();
        Equalizer.ReapplyAll();
        Sync.ReapplyAll();
    }
}
