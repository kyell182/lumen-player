namespace Lumen.Features.Convert;

public enum ConversionResult
{
    Completed,
    Failed,
    Cancelled,
}

public interface IConversionRunner
{
    /// <summary>Zet <paramref name="source"/> om naar <paramref name="destination"/>. Voortgang van 0 tot 100.</summary>
    Task<ConversionResult> RunAsync(
        string source, ConversionProfile profile, string destination, IProgress<double> progress, CancellationToken cancellationToken);
}
