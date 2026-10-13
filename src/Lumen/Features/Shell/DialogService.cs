using Avalonia.Controls;
using Lumen.Features.Convert;
using Lumen.Features.Convert.Views;
using Lumen.Features.Network.Views;

namespace Lumen.Features.Shell;

public sealed class DialogService : IDialogService
{
    private readonly Func<Window?> _owner;
    private readonly ConvertViewModel _converter;
    private ConvertWindow? _convertWindow;

    public DialogService(Func<Window?> owner, ConvertViewModel converter)
    {
        _owner = owner;
        _converter = converter;
    }

    public async Task<string?> AskStreamUrlAsync()
    {
        if (_owner() is not { } owner) return null;
        return await new StreamUrlWindow().ShowDialog<string?>(owner);
    }

    public void ShowConverter(IReadOnlyList<string>? sources = null)
    {
        if (_convertWindow is null)
        {
            _convertWindow = new ConvertWindow(_converter);
            _convertWindow.Closed += (_, _) => _convertWindow = null;
            if (_owner() is { } owner) _convertWindow.Show(owner);
            else _convertWindow.Show();
        }
        else
        {
            _convertWindow.Activate();
        }

        if (sources is { Count: > 0 }) _converter.Enqueue(sources);
    }
}
