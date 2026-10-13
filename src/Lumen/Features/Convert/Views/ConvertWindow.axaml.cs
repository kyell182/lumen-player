using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;

namespace Lumen.Features.Convert.Views;

public partial class ConvertWindow : Window
{
    public ConvertWindow() => InitializeComponent();

    public ConvertWindow(ConvertViewModel viewModel) : this()
    {
        DataContext = viewModel;
        AddHandler(DragDrop.DropEvent, (_, e) => OnDrop(viewModel, e));
    }

#pragma warning disable CS0618 // klassieke drag-and-drop-API, werkt in alle Avalonia 11-versies
    private static void OnDrop(ConvertViewModel viewModel, DragEventArgs e)
    {
        var paths = e.Data.GetFiles()?.Select(f => f.TryGetLocalPath()).OfType<string>().ToList();
        if (paths is { Count: > 0 }) viewModel.Enqueue(Infrastructure.MediaFileTypes.Expand(paths));
    }
#pragma warning restore CS0618
}
