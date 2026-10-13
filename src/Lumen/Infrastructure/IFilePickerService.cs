namespace Lumen.Infrastructure;

public interface IFilePickerService
{
    Task<IReadOnlyList<string>> PickMediaFilesAsync();

    Task<string?> PickFolderAsync();

    Task<string?> PickSubtitleAsync();

    Task<string?> PickPlaylistAsync();

    /// <summary>Vraagt waar een bestand opgeslagen moet worden. Null als de gebruiker annuleert.</summary>
    Task<string?> PickSaveFileAsync(string title, string suggestedName, string typeName, string extension);
}
