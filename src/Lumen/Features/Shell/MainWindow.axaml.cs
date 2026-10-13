using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Lumen.Features.Playback.Video;

namespace Lumen.Features.Shell;

public partial class MainWindow : Window
{
    // Ruimte voor de minimaliseer/maximaliseer/sluit-knoppen van Windows.
    private const double WindowsCaptionButtonsWidth = 140;

    private readonly KeyboardShortcuts? _shortcuts;
    private WindowState _stateBeforeFullScreen = WindowState.Normal;

    public MainWindow() => InitializeComponent();

    private static readonly Cursor HiddenCursor = new(StandardCursorType.None);

    public MainWindow(MainViewModel viewModel, IVideoFrameSource video) : this()
    {
        DataContext = viewModel;
        _shortcuts = new KeyboardShortcuts(viewModel);
        PlayerSurface.AttachVideo(video);
        PlayerSurface.Clicked += (_, _) => viewModel.Player.TogglePlayPause();
        PlayerSurface.DoubleClicked += (_, _) => viewModel.ToggleFullScreenCommand.Execute(null);

        SubtitleLayer.SetRaised(true);
        ConfigureTitleBar();
        WireInput(viewModel);
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Closing += (_, _) => viewModel.Shutdown();
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    private void ConfigureTitleBar()
    {
        var rightMargin = OperatingSystem.IsWindows() ? WindowsCaptionButtonsWidth : 8;
        TitleBarContent.Margin = new Thickness(14, 0, rightMargin, 0);

        TitleBar.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
        };
        TitleBar.DoubleTapped += (_, _) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void WireInput(MainViewModel viewModel)
    {
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, (_, _) => viewModel.ReportUserActivity(),
            RoutingStrategies.Tunnel, handledEventsToo: true);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_shortcuts?.TryHandle(e.Key, e.KeyModifiers) == true)
        {
            e.Handled = true;
        }
    }

    // De nieuwe DataTransfer-API bestaat pas vanaf recente Avalonia-versies; de klassieke API werkt overal.
#pragma warning disable CS0618
    private static void OnDragOver(object? sender, DragEventArgs e) =>
        e.DragEffects = e.Data.Contains(DataFormats.Files) ? DragDropEffects.Copy : DragDropEffects.None;

    private void OnDrop(object? sender, DragEventArgs e)
    {
        var paths = e.Data.GetFiles()?
            .Select(item => item.TryGetLocalPath())
            .OfType<string>()
            .ToList();

        if (paths is { Count: > 0 })
        {
            ViewModel?.HandleDroppedPaths(paths);
        }
    }
#pragma warning restore CS0618

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (ViewModel is not { } vm) return;

        switch (e.PropertyName)
        {
            case nameof(MainViewModel.IsFullScreen):
                ApplyFullScreen(vm.IsFullScreen);
                break;
            case nameof(MainViewModel.ControlsVisible):
                // Muisaanwijzer verbergen samen met de bediening.
                PlayerArea.Cursor = vm.ControlsVisible ? Cursor.Default : HiddenCursor;
                SubtitleLayer.SetRaised(vm.ControlsVisible);
                break;
        }
    }

    private void ApplyFullScreen(bool fullScreen)
    {
        if (fullScreen)
        {
            _stateBeforeFullScreen = WindowState;
            WindowState = WindowState.FullScreen;
        }
        else if (WindowState == WindowState.FullScreen)
        {
            WindowState = _stateBeforeFullScreen == WindowState.FullScreen ? WindowState.Normal : _stateBeforeFullScreen;
        }
    }
}
