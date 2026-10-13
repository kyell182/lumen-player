using Avalonia.Controls;

namespace Lumen.Features.Playback.Views;

/// <summary>Toont de ondertiteltekst over de video; de lettergrootte schaalt mee met het venster.</summary>
public partial class SubtitleOverlay : UserControl
{
    private const double FontToHeightRatio = 0.042;
    private const double MinFontSize = 16;
    private const double MaxFontSize = 54;
    private const double MaxWidthRatio = 0.85;

    public SubtitleOverlay()
    {
        InitializeComponent();
        SizeChanged += (_, e) => ScaleTo(e.NewSize.Width, e.NewSize.Height);
    }

    /// <summary>Schuift de ondertitels omhoog zodat ze niet achter de bediening verdwijnen.</summary>
    public void SetRaised(bool raised) => Classes.Set("raised", raised);

    private void ScaleTo(double width, double height)
    {
        CueText.FontSize = Math.Clamp(height * FontToHeightRatio, MinFontSize, MaxFontSize);
        Cue.MaxWidth = width * MaxWidthRatio;
    }
}
