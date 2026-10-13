using LibVLCSharp.Shared;
using Lumen.Features.Playback.Engine;
using Lumen.Infrastructure;

namespace Lumen.Features.Convert;

/// <summary>
/// Converteert met een aparte, onzichtbare VLC-speler die naar een bestand schrijft in plaats van naar het scherm.
/// Het afspelen in Lumen gaat ondertussen gewoon door.
/// </summary>
public sealed class VlcConversionRunner : IConversionRunner
{
    private readonly LibVLC _libVlc;

    public VlcConversionRunner(VlcEngine engine) => _libVlc = engine.LibVlc;

    public async Task<ConversionResult> RunAsync(
        string source, ConversionProfile profile, string destination, IProgress<double> progress, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        using var media = new Media(_libVlc, MediaSource.ToUri(source));
        media.AddOption(":sout=" + profile.BuildStreamOutput(destination));
        media.AddOption(":sout-keep");
        if (profile.AudioOnly) media.AddOption(":no-sout-video");

        using var player = new MediaPlayer(media);
        var finished = new TaskCompletionSource<ConversionResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Alleen het resultaat doorgeven: libVLC nooit aanroepen vanuit zijn eigen events.
        player.PositionChanged += (_, e) => progress.Report(Math.Clamp(e.Position * 100, 0, 100));
        player.EndReached += (_, _) => finished.TrySetResult(ConversionResult.Completed);
        player.EncounteredError += (_, _) => finished.TrySetResult(ConversionResult.Failed);

        await using var registration = cancellationToken.Register(() => finished.TrySetResult(ConversionResult.Cancelled));

        player.Play();
        var result = await finished.Task;
        await Task.Run(player.Stop);

        if (result == ConversionResult.Completed) progress.Report(100);
        return result;
    }
}
