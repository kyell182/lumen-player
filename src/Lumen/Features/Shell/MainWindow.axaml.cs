using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Lumen.Features.Playback.Video;
using Lumen.Features.Shell.Views;

namespace Lumen.Features.Shell;

public partial class MainWindow : Window
{
    // Ruimte voor de minimaliseer/maximaliseer/sluit-knoppen van Windows.
    private const double WindowsCaptionButtonsWidth = 140;

    private readonly KeyboardShortcuts? _shortcuts;
    private readonly PlayerOverlay? _overlay;
    private WindowState _stateBeforeFullScreen = WindowState.Normal;

    public MainWindow() => InitializeComponent();

    public MainWindow(MainViewModel viewModel, VideoOutput video) : this()
    {
        DataContext = viewModel;
        _shortcuts = new KeyboardShortcuts(viewModel);

        PlayerSurface.Show(video);
        _overlay = CreateOverlay(viewModel);
        PlaceOverlay(_overlay);

        ConfigureTitleBar();
        AppearanceButton.Click += (_, _) => AppearanceMenu.Show(AppearanceButton, viewModel.Theme, viewModel.Skins);
        WireInput(this, viewModel);
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Closing += (_, _) => viewModel.Shutdown();
    }

    private MainViewModel? ViewModel => DataContext as MainViewModel;

    private static PlayerOverlay CreateOverlay(MainViewModel viewModel)
    {
        var overlay = new PlayerOverlay { DataContext = viewModel };
        overlay.Clicked += (_, _) => viewModel.Player.TogglePlayPause();
        overlay.DoubleClicked += (_, _) => viewModel.ToggleFullScreenCommand.Execute(null);
        overlay.SetControlsVisible(true);
        return overlay;
    }

    // Over een native videovenster kan Avalonia niet tekenen: dan komt de bediening in een eigen doorzichtig venster.
    private void PlaceOverlay(PlayerOverlay overlay)
    {
        if (!PlayerSurface.UsesNativeWindow)
        {
            PlayerArea.Children.Add(overlay);
            return;
        }

        var overlayWindow = new OverlayWindow(this, PlayerArea, overlay);
        if (ViewModel is { } vm) WireInput(overlayWindow, vm);
    }

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

    // Toetsen, muisbeweging en slepen werken zowel in het hoofdvenster als in het overlay-venster.
    private void WireInput(Window window, MainViewModel viewModel)
    {
        DragDrop.SetAllowDrop(window, true);
        window.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        window.AddHandler(PointerMovedEvent, (_, _) => viewModel.ReportUserActivity(),
            RoutingStrategies.Tunnel, handledEventsToo: true);
        window.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        window.AddHandler(DragDrop.DropEvent, OnDrop);
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
                _overlay?.SetControlsVisible(vm.ControlsVisible);
                break;
        }
    }

    private void ApplyFullScreen(bool fullScreen)
    {
        if (fullScreen)
        {
            _stateBeforeFullScreen = WindowState;
            // Zonder dit blijft de bovenrand van het Windows-venster (in je accentkleur) zichtbaar.
            ExtendClientAreaToDecorationsHint = false;
            SystemDecorations = SystemDecorations.None;
            WindowState = WindowState.FullScreen;
        }
        else if (WindowState == WindowState.FullScreen)
        {
            WindowState = _stateBeforeFullScreen == WindowState.FullScreen ? WindowState.Normal : _stateBeforeFullScreen;
            SystemDecorations = SystemDecorations.Full;
            ExtendClientAreaToDecorationsHint = true;
        }
    }
}
