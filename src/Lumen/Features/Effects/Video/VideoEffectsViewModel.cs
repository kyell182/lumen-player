using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lumen.Features.Effects.Video;

/// <summary>Tabblad "Beeld": kleur, beeldverhouding, bijsnijden, zoom, draaien en deinterlacen.</summary>
public sealed partial class VideoEffectsViewModel : ObservableObject
{
    private readonly IVideoEffects _effects;

    [ObservableProperty] private double _brightness = 1;
    [ObservableProperty] private double _contrast = 1;
    [ObservableProperty] private double _saturation = 1;
    [ObservableProperty] private double _hue;
    [ObservableProperty] private double _gamma = 1;
    [ObservableProperty] private VideoOption<string?> _aspectRatio = VideoOptions.AspectRatios[0];
    [ObservableProperty] private VideoOption<string?> _crop = VideoOptions.Crops[0];
    [ObservableProperty] private VideoOption<float> _zoom = VideoOptions.Zooms[0];
    [ObservableProperty] private VideoOption<string?> _rotation = VideoOptions.Rotations[0];
    [ObservableProperty] private bool _deinterlace;

    public VideoEffectsViewModel(IVideoEffects effects) => _effects = effects;

    public IReadOnlyList<VideoOption<string?>> AspectRatios => VideoOptions.AspectRatios;
    public IReadOnlyList<VideoOption<string?>> Crops => VideoOptions.Crops;
    public IReadOnlyList<VideoOption<float>> Zooms => VideoOptions.Zooms;
    public IReadOnlyList<VideoOption<string?>> Rotations => VideoOptions.Rotations;

    /// <summary>Na het openen van een nieuw bestand alles opnieuw toepassen.</summary>
    public void ReapplyAll()
    {
        ApplyColor();
        _effects.SetAspectRatio(AspectRatio.Value);
        _effects.SetCrop(Crop.Value);
        _effects.SetZoom(Zoom.Value);
        _effects.SetDeinterlace(Deinterlace);
    }

    [RelayCommand]
    private void Reset()
    {
        Brightness = 1;
        Contrast = 1;
        Saturation = 1;
        Hue = 0;
        Gamma = 1;
        AspectRatio = AspectRatios[0];
        Crop = Crops[0];
        Zoom = Zooms[0];
        Rotation = Rotations[0];
        Deinterlace = false;
    }

    partial void OnBrightnessChanged(double value) => ApplyColor();
    partial void OnContrastChanged(double value) => ApplyColor();
    partial void OnSaturationChanged(double value) => ApplyColor();
    partial void OnHueChanged(double value) => ApplyColor();
    partial void OnGammaChanged(double value) => ApplyColor();
    partial void OnAspectRatioChanged(VideoOption<string?> value) => _effects.SetAspectRatio(value.Value);
    partial void OnCropChanged(VideoOption<string?> value) => _effects.SetCrop(value.Value);
    partial void OnZoomChanged(VideoOption<float> value) => _effects.SetZoom(value.Value);
    partial void OnRotationChanged(VideoOption<string?> value) => _effects.SetRotation(value.Value);
    partial void OnDeinterlaceChanged(bool value) => _effects.SetDeinterlace(value);

    private void ApplyColor() => _effects.SetColor(new ColorAdjustments(
        (float)Brightness, (float)Contrast, (float)Saturation, (float)Hue, (float)Gamma));
}
