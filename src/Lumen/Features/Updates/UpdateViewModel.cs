using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lumen.Infrastructure;

namespace Lumen.Features.Updates;

public enum UpdateStatus
{
    None,
    Available,
    Downloading,
    Installing,
    Failed,
}

/// <summary>De updatemelding bovenaan het venster.</summary>
public sealed partial class UpdateViewModel : ObservableObject
{
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(4);

    private readonly UpdateService _updates;
    private readonly Action _shutdownApp;
    private UpdateInfo? _update;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible), nameof(IsAvailable), nameof(IsDownloading), nameof(IsFailed), nameof(CanDismiss))]
    private UpdateStatus _status;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private double _progressPercent;

    public UpdateViewModel(UpdateService updates, Action shutdownApp)
    {
        _updates = updates;
        _shutdownApp = shutdownApp;
    }

    public bool IsVisible => Status != UpdateStatus.None;

    public bool IsAvailable => Status == UpdateStatus.Available;

    public bool IsDownloading => Status == UpdateStatus.Downloading;

    public bool IsFailed => Status == UpdateStatus.Failed;

    public bool CanDismiss => Status is UpdateStatus.Available or UpdateStatus.Failed;

    /// <summary>Kijkt kort na het opstarten stil of er een nieuwe versie is.</summary>
    public async Task CheckOnStartupAsync()
    {
        await Task.Delay(StartupDelay);
        try
        {
            _update = await _updates.FindUpdateAsync();
            if (_update is null) return;

            Message = $"Lumen {_update.Version.ToString(3)} is beschikbaar (je hebt {AppVersion.Current.ToString(3)})";
            Status = UpdateStatus.Available;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            // Geen internet of GitHub onbereikbaar: gewoon niets tonen.
        }
    }

    [RelayCommand]
    private async Task InstallAsync()
    {
        if (_update is null) return;

        try
        {
            Status = UpdateStatus.Downloading;
            Message = $"Lumen {_update.Version.ToString(3)} downloaden…";
            var progress = new Progress<double>(p => ProgressPercent = p);
            var installer = await _updates.DownloadAsync(_update, progress);

            Status = UpdateStatus.Installing;
            Message = "Installeren… Lumen start zo opnieuw op.";
            if (await _updates.InstallAsync(installer))
            {
                _shutdownApp();
                return;
            }

            Fail("Bijwerken is niet gelukt. Je kan de nieuwe versie handmatig downloaden.");
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or UnauthorizedAccessException)
        {
            CrashLogger.Write("Update mislukt", ex);
            Fail("Downloaden is mislukt. Controleer je internetverbinding.");
        }
    }

    [RelayCommand]
    private void OpenReleasePage()
    {
        if (_update is not null) ExternalLauncher.OpenUrl(_update.PageUrl);
    }

    [RelayCommand]
    private void Dismiss() => Status = UpdateStatus.None;

    private void Fail(string message)
    {
        Message = message;
        Status = UpdateStatus.Failed;
    }
}
