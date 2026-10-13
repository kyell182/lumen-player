using Avalonia.Controls;
using Lumen.Infrastructure;

namespace Lumen.Features.Network.Views;

/// <summary>Vraagt een netwerkadres. ShowDialog geeft het adres terug, of null bij annuleren.</summary>
public partial class StreamUrlWindow : Window
{
    public StreamUrlWindow()
    {
        InitializeComponent();
        CancelButton.Click += (_, _) => Close(null);
        PlayButton.Click += (_, _) => Confirm();
        Opened += (_, _) => UrlBox.Focus();
    }

    private void Confirm()
    {
        var url = UrlBox.Text?.Trim() ?? string.Empty;
        if (!MediaSource.IsStream(url))
        {
            ErrorText.IsVisible = true;
            return;
        }

        Close(url);
    }
}
