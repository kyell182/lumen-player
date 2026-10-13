using Lumen.Infrastructure;

namespace Lumen.Features.Convert;

/// <summary>
/// Neemt een netwerkstream op naar een bestand (in Video's\Lumen), los van wat er afgespeeld wordt.
/// Lokale bestanden opnemen heeft geen zin; daarvoor is Converteren.
/// </summary>
public sealed class StreamRecorder
{
    private const string OutputSuffix = "opname";

    private readonly IConversionRunner _runner;
    private CancellationTokenSource? _cancellation;

    public StreamRecorder(IConversionRunner runner) => _runner = runner;

    public bool IsRecording => _cancellation is not null;

    public static bool CanRecord(string? source) => source is not null && MediaSource.IsStream(source);

    /// <summary>Start de opname en geeft het doelbestand terug. De taak eindigt wanneer de opname stopt.</summary>
    public string Start(string source, Action<ConversionResult> onFinished)
    {
        var destination = OutputPaths.For(source, OutputPaths.DefaultFolder, OutputSuffix, ConversionProfiles.Recording.Extension);
        var cancellation = new CancellationTokenSource();
        _cancellation = cancellation;

        _ = RecordAsync(source, destination, cancellation, onFinished);
        return destination;
    }

    public void Stop() => _cancellation?.Cancel();

    private async Task RecordAsync(string source, string destination, CancellationTokenSource cancellation, Action<ConversionResult> onFinished)
    {
        var result = await _runner.RunAsync(source, ConversionProfiles.Recording, destination, new Progress<double>(), cancellation.Token);
        if (_cancellation == cancellation) _cancellation = null;
        cancellation.Dispose();
        onFinished(result);
    }
}
