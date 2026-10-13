using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lumen.Infrastructure;

namespace Lumen.Features.Convert;

/// <summary>Eén bestand in de conversielijst, met voortgang en status.</summary>
public sealed partial class ConversionJobViewModel : ObservableObject
{
    private readonly CancellationTokenSource _cancellation = new();

    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _status = "In de wachtrij";
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isFinished;

    public ConversionJobViewModel(string source, ConversionProfile profile, string destination)
    {
        Source = source;
        Profile = profile;
        Destination = destination;
    }

    public string Source { get; }

    public ConversionProfile Profile { get; }

    public string Destination { get; }

    public string Title => MediaSource.DisplayName(Source);

    public string DestinationName => Path.GetFileName(Destination);

    public CancellationToken CancellationToken => _cancellation.Token;

    public void MarkStarted()
    {
        IsRunning = true;
        Status = "Bezig…";
    }

    public void MarkFinished(ConversionResult result)
    {
        IsRunning = false;
        IsFinished = true;
        Status = result switch
        {
            ConversionResult.Completed => "Klaar",
            ConversionResult.Cancelled => "Geannuleerd",
            _ => "Mislukt (zie vlc.log)",
        };
    }

    [RelayCommand]
    private void Cancel() => _cancellation.Cancel();

    [RelayCommand]
    private void ShowInFolder()
    {
        if (File.Exists(Destination)) ExternalLauncher.OpenUrl(Path.GetDirectoryName(Destination)!);
    }
}
