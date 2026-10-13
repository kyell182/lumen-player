namespace Lumen.Features.Cast;

/// <summary>Een apparaat waarnaar je kan casten, bv. een Chromecast.</summary>
public sealed record CastDevice(Guid Id, string Name, string Type);
