using Lumen.Infrastructure;

namespace Lumen.Features.Playlist;

/// <summary>Iets om af te spelen: een bestand of netwerkadres, met eventueel een eigen titel (bv. uit een .m3u).</summary>
public sealed record PlaylistEntry(string Source, string? Title = null)
{
    public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? MediaSource.DisplayName(Source) : Title!;
}
