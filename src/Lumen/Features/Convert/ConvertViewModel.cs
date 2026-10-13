using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lumen.Infrastructure;

namespace Lumen.Features.Convert;

/// <summary>Het venster "Converteren": bestanden kiezen, doelformaat kiezen, en ze één voor één omzetten.</summary>
public sealed partial class ConvertViewModel : ObservableObject
{
    private const string OutputSuffix = "geconverteerd";

    private readonly IConversionRunner _runner;
    private readonly IFilePickerService _picker;
    private bool _isProcessing;

    [ObservableProperty]
    private ConversionProfile _selectedProfile = ConversionProfiles.All[0];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OutputFolderText))]
    private string? _outputFolder;

    public ConvertViewModel(IConversionRunner runner, IFilePickerService picker)
    {
        _runner = runner;
        _picker = picker;
    }

    public IReadOnlyList<ConversionProfile> Profiles => ConversionProfiles.All;

    public ObservableCollection<ConversionJobViewModel> Jobs { get; } = new();

    public string OutputFolderText => OutputFolder ?? "Zelfde map als het origineel";

    /// <summary>Zet bestanden in de wachtrij met het gekozen formaat en start als er nog niets loopt.</summary>
    public void Enqueue(IEnumerable<string> sources)
    {
        foreach (var source in sources)
        {
            var destination = OutputPaths.For(source, OutputFolder, OutputSuffix, SelectedProfile.Extension);
            Jobs.Add(new ConversionJobViewModel(source, SelectedProfile, destination));
        }

        _ = ProcessQueueAsync();
    }

    [RelayCommand]
    private async Task AddFilesAsync()
    {
        var files = await _picker.PickMediaFilesAsync();
        if (files.Count > 0) Enqueue(MediaFileTypes.Expand(files));
    }

    [RelayCommand]
    private async Task ChooseFolderAsync()
    {
        if (await _picker.PickFolderAsync() is { } folder) OutputFolder = folder;
    }

    [RelayCommand]
    private void ResetFolder() => OutputFolder = null;

    [RelayCommand]
    private void ClearFinished()
    {
        foreach (var job in Jobs.Where(j => j.IsFinished).ToList()) Jobs.Remove(job);
    }

    private async Task ProcessQueueAsync()
    {
        if (_isProcessing) return;
        _isProcessing = true;

        try
        {
            while (Jobs.FirstOrDefault(j => !j.IsFinished && !j.IsRunning) is { } job)
            {
                job.MarkStarted();
                var progress = new Progress<double>(p => job.Progress = p);
                var result = await _runner.RunAsync(job.Source, job.Profile, job.Destination, progress, job.CancellationToken);
                job.MarkFinished(result);
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }
}
