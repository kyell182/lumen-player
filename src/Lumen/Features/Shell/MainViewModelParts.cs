using Lumen.Features.Cast;
using Lumen.Features.Effects;
using Lumen.Features.Home;
using Lumen.Features.MediaInfo;
using Lumen.Features.Playback;
using Lumen.Features.Playlist;
using Lumen.Features.Settings;
using Lumen.Features.Settings.Skins;
using Lumen.Features.Tools;
using Lumen.Features.Updates;

namespace Lumen.Features.Shell;

/// <summary>De deel-viewmodels die het hoofdvenster toont (houdt de constructor van MainViewModel overzichtelijk).</summary>
public sealed record MainViewModelParts(
    PlayerViewModel Player,
    PlaylistViewModel Playlist,
    HomeViewModel Home,
    UpdateViewModel Update,
    EffectsViewModel Effects,
    MediaInfoViewModel MediaInfo,
    CastViewModel Cast,
    ToolsViewModel Tools,
    ThemeService Theme,
    SkinService Skins);
