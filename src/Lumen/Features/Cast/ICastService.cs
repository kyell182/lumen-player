namespace Lumen.Features.Cast;

public interface ICastService
{
    /// <summary>De lijst apparaten is veranderd. Komt binnen op een achtergrondthread.</summary>
    event EventHandler? DevicesChanged;

    IReadOnlyList<CastDevice> Devices { get; }

    /// <summary>Waarnaar nu gecast wordt, of null bij afspelen op deze computer.</summary>
    CastDevice? Current { get; }

    void StartDiscovery();

    /// <summary>Cast naar dit apparaat, of terug naar deze computer met null.</summary>
    void CastTo(CastDevice? device);
}
