using CommunityToolkit.Mvvm.ComponentModel;
using Lumen.Infrastructure;

namespace Lumen.Features.Playlist;

public sealed partial class PlaylistItemViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowVideoIcon), nameof(ShowAudioIcon))]
    private bool _isCurrent;

    public PlaylistItemViewModel(string path)
    {
        Path = path;
        Title = System.IO.Path.GetFileNameWithoutExtension(path);
        Kind = MediaFileTypes.KindOf(path);
    }

    public string Path { get; }

    public string Title { get; }

    public MediaKind Kind { get; }

    public bool ShowVideoIcon => !IsCurrent && Kind != MediaKind.Audio;

    public bool ShowAudioIcon => !IsCurrent && Kind == MediaKind.Audio;
}
