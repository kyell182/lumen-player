namespace Lumen.Infrastructure;

public interface IFilePickerService
{
    Task<IReadOnlyList<string>> PickMediaFilesAsync();

    Task<string?> PickFolderAsync();

    Task<string?> PickSubtitleAsync();
}
