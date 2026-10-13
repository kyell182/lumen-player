using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Lumen.Infrastructure;

/// <summary>Opent de systeemdialogen voor bestanden en mappen.</summary>
public sealed class FilePickerService : IFilePickerService
{
    private static readonly FilePickerFileType AllMedia = Filter(
        "Alle media", MediaFileTypes.VideoExtensions.Concat(MediaFileTypes.AudioExtensions).Concat(MediaFileTypes.PlaylistExtensions));

    private static readonly FilePickerFileType Playlists = Filter("Afspeellijsten", MediaFileTypes.PlaylistExtensions);

    private static readonly FilePickerFileType Video = Filter("Video", MediaFileTypes.VideoExtensions);
    private static readonly FilePickerFileType Audio = Filter("Muziek", MediaFileTypes.AudioExtensions);
    private static readonly FilePickerFileType Subtitles = Filter("Ondertitels", MediaFileTypes.SubtitleExtensions);

    private readonly Func<TopLevel?> _topLevel;

    public FilePickerService(Func<TopLevel?> topLevel) => _topLevel = topLevel;

    public async Task<IReadOnlyList<string>> PickMediaFilesAsync()
    {
        if (Storage is not { } storage) return Array.Empty<string>();

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Media openen",
            AllowMultiple = true,
            FileTypeFilter = new[] { AllMedia, Video, Audio, FilePickerFileTypes.All },
        });

        return LocalPaths(files);
    }

    public async Task<string?> PickFolderAsync()
    {
        if (Storage is not { } storage) return null;

        var folders = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Map openen",
            AllowMultiple = false,
        });

        return folders.Select(f => f.TryGetLocalPath()).FirstOrDefault(p => p is not null);
    }

    public async Task<string?> PickSubtitleAsync()
    {
        if (Storage is not { } storage) return null;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Ondertitelbestand laden",
            AllowMultiple = false,
            FileTypeFilter = new[] { Subtitles, FilePickerFileTypes.All },
        });

        return LocalPaths(files).FirstOrDefault();
    }

    public async Task<string?> PickPlaylistAsync()
    {
        if (Storage is not { } storage) return null;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Afspeellijst openen",
            AllowMultiple = false,
            FileTypeFilter = new[] { Playlists, FilePickerFileTypes.All },
        });

        return LocalPaths(files).FirstOrDefault();
    }

    public async Task<string?> PickSaveFileAsync(string title, string suggestedName, string typeName, string extension)
    {
        if (Storage is not { } storage) return null;

        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedName,
            DefaultExtension = extension,
            ShowOverwritePrompt = true,
            FileTypeChoices = new[] { Filter(typeName, new[] { extension }) },
        });

        return file?.TryGetLocalPath();
    }

    private IStorageProvider? Storage => _topLevel()?.StorageProvider;

    private static IReadOnlyList<string> LocalPaths(IEnumerable<IStorageItem> items) =>
        items.Select(i => i.TryGetLocalPath()).OfType<string>().ToList();

    private static FilePickerFileType Filter(string name, IEnumerable<string> extensions) =>
        new(name) { Patterns = extensions.Select(e => "*." + e).ToArray() };
}
