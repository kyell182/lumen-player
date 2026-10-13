namespace Lumen.Features.Updates;

public interface IReleaseSource
{
    /// <summary>Haalt de nieuwste release op, of null als dat niet lukt.</summary>
    Task<ReleaseInfo?> GetLatestAsync(CancellationToken cancellationToken);
}
