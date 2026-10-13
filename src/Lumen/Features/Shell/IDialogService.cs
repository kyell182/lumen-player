namespace Lumen.Features.Shell;

/// <summary>Opent de extra vensters van Lumen, zodat viewmodels geen vensters hoeven te kennen.</summary>
public interface IDialogService
{
    /// <summary>Vraagt een netwerkadres; null bij annuleren.</summary>
    Task<string?> AskStreamUrlAsync();

    /// <summary>Toont het venster "Converteren", eventueel met bestanden al in de wachtrij.</summary>
    void ShowConverter(IReadOnlyList<string>? sources = null);
}
