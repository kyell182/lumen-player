using CommunityToolkit.Mvvm.ComponentModel;
using Lumen.Infrastructure;

namespace Lumen.Features.Playlist;

public sealed partial class PlaylistItemViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowVideoIcon), nameof(ShowAudioIcon), nameof(ShowStreamIcon))]
    private bool _isCurrent;

    public PlaylistItemViewModel(PlaylistEntry entry)
    {
        Entry = entry;
        Kind = MediaFileTypes.KindOf(entry.Source);
        IsStream = MediaSource.IsStream(entry.Source);
    }

    public PlaylistEntry Entry { get; }

    /// <summary>Bestandspad of netwerkadres.</summary>
    public string Source => Entry.Source;

    public string Title => Entry.DisplayTitle;

    public MediaKind Kind { get; }

    public bool IsStream { get; }

    public bool ShowStreamIcon => !IsCurrent && IsStream;

    public bool ShowVideoIcon => !IsCurrent && !IsStream && Kind != MediaKind.Audio;

    public bool ShowAudioIcon => !IsCurrent && !IsStream && Kind == MediaKind.Audio;
}
