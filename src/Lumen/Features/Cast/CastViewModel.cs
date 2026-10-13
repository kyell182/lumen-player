using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lumen.Features.Cast;

/// <summary>De castknop: welke apparaten er zijn en of er gecast wordt.</summary>
public sealed partial class CastViewModel : ObservableObject
{
    private readonly ICastService _cast;

    [ObservableProperty]
    private bool _isCasting;

    public CastViewModel(ICastService cast)
    {
        _cast = cast;
        _cast.DevicesChanged += (_, _) => Dispatcher.UIThread.Post(() => OnPropertyChanged(nameof(Devices)));
    }

    public IReadOnlyList<CastDevice> Devices => _cast.Devices;

    public CastDevice? Current => _cast.Current;

    /// <summary>Begint te zoeken zodra de gebruiker het castmenu voor het eerst opent.</summary>
    public void EnsureDiscovery() => _cast.StartDiscovery();

    public void CastTo(CastDevice? device)
    {
        _cast.CastTo(device);
        IsCasting = _cast.Current is not null;
        OnPropertyChanged(nameof(Current));
    }
}
