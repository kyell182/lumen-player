namespace Lumen.Features.Effects.Video;

/// <summary>Kleurcorrectie zoals VLC ze kent. Standaardwaarden = geen aanpassing.</summary>
public sealed record ColorAdjustments(float Brightness, float Contrast, float Saturation, float Hue, float Gamma)
{
    public static readonly ColorAdjustments Default = new(1f, 1f, 1f, 0f, 1f);

    public bool IsDefault => this == Default;
}
