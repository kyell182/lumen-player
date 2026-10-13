namespace Lumen.Features.MediaInfo;

public interface IMediaInfoSource
{
    /// <summary>Technische informatie over wat er nu speelt. Leeg als er niets speelt.</summary>
    IReadOnlyList<InfoSection> Read();
}
