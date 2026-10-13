namespace Lumen.Features.MediaInfo;

public sealed record InfoRow(string Label, string Value);

/// <summary>Een blok in het infopaneel, bv. "Video" met codec en resolutie.</summary>
public sealed record InfoSection(string Title, IReadOnlyList<InfoRow> Rows);
