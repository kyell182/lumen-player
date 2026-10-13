using Lumen.Features.Effects.Video;

namespace Lumen.Features.Tools;

/// <summary>Bewaart het huidige videobeeld als PNG in Afbeeldingen\Lumen.</summary>
public sealed class SnapshotService
{
    private readonly IVideoEffects _video;

    public SnapshotService(IVideoEffects video) => _video = video;

    public static string Folder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Lumen");

    /// <summary>Het pad van de schermafbeelding, of null als het niet lukte (bv. bij muziek).</summary>
    public string? Take()
    {
        Directory.CreateDirectory(Folder);
        var path = Path.Combine(Folder, $"Lumen {DateTime.Now:yyyy-MM-dd HH.mm.ss}.png");
        return _video.TakeSnapshot(path) ? path : null;
    }
}
