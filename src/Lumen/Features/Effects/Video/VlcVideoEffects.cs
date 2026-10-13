using LibVLCSharp.Shared;
using Lumen.Features.Playback;
using Lumen.Features.Playback.Engine;

namespace Lumen.Features.Effects.Video;

/// <summary>Beeldaanpassingen via de VLC-speler.</summary>
public sealed class VlcVideoEffects : IVideoEffects
{
    private const string RotationOptionKey = "rotation";
    private const string DeinterlaceMode = "yadif";

    private readonly MediaPlayer _player;
    private readonly IMediaReloader _reloader;
    private string? _rotation;

    public VlcVideoEffects(VlcEngine engine, IMediaReloader reloader)
    {
        _player = engine.Player;
        _reloader = reloader;
    }

    public void SetColor(ColorAdjustments adjustments)
    {
        if (adjustments.IsDefault)
        {
            _player.SetAdjustInt(VideoAdjustOption.Enable, 0);
            return;
        }

        _player.SetAdjustInt(VideoAdjustOption.Enable, 1);
        _player.SetAdjustFloat(VideoAdjustOption.Brightness, adjustments.Brightness);
        _player.SetAdjustFloat(VideoAdjustOption.Contrast, adjustments.Contrast);
        _player.SetAdjustFloat(VideoAdjustOption.Saturation, adjustments.Saturation);
        _player.SetAdjustFloat(VideoAdjustOption.Hue, adjustments.Hue);
        _player.SetAdjustFloat(VideoAdjustOption.Gamma, adjustments.Gamma);
    }

    public void SetAspectRatio(string? ratio) => _player.AspectRatio = ratio;

    public void SetCrop(string? ratio) => _player.CropGeometry = ratio;

    public void SetZoom(float scale) => _player.Scale = scale;

    public void SetDeinterlace(bool enabled) => _player.SetDeinterlace(enabled ? DeinterlaceMode : null);

    public void SetRotation(string? transform)
    {
        if (transform == _rotation) return;
        _rotation = transform;

        _reloader.SetMediaOption(RotationOptionKey, transform is null ? null : $":video-filter=transform{{type={transform}}}");
        _reloader.Reload();
    }

    public bool TakeSnapshot(string filePath) => _player.TakeSnapshot(0, filePath, 0, 0);
}
