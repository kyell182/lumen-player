using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Lumen.Features.Playback.Video;

namespace Lumen.Features.Playback.Views;

/// <summary>Tekent de videoframes als gewone Avalonia-afbeelding, zodat er andere controls over kunnen liggen.</summary>
public sealed class VideoSurface : Image
{
    private static readonly Vector Dpi = new(96, 96);

    private IVideoFrameSource? _source;
    private WriteableBitmap? _bitmap;
    private int _presentPending;

    public VideoSurface()
    {
        Stretch = Stretch.Uniform;
    }

    public void Attach(IVideoFrameSource source)
    {
        _source = source;
        _source.FrameReady += OnFrameReady;
    }

    // Komt binnen op de videothread: hooguit één tekenopdracht tegelijk inplannen.
    private void OnFrameReady(object? sender, EventArgs e)
    {
        if (Interlocked.Exchange(ref _presentPending, 1) == 0)
        {
            Dispatcher.UIThread.Post(Present, DispatcherPriority.Render);
        }
    }

    private void Present()
    {
        Interlocked.Exchange(ref _presentPending, 0);
        if (_source?.ReadLatestFrame(CopyIntoBitmap) != true)
        {
            Source = null;
        }
    }

    private void CopyIntoBitmap(byte[] pixels, int width, int height, int stride)
    {
        var bitmap = EnsureBitmap(width, height);
        using (var buffer = bitmap.Lock())
        {
            var rowBytes = width * 4;
            for (var row = 0; row < height; row++)
            {
                Marshal.Copy(pixels, row * stride, buffer.Address + row * buffer.RowBytes, rowBytes);
            }
        }

        InvalidateVisual();
    }

    private WriteableBitmap EnsureBitmap(int width, int height)
    {
        if (_bitmap is not null && _bitmap.PixelSize.Width == width && _bitmap.PixelSize.Height == height)
        {
            return _bitmap;
        }

        _bitmap?.Dispose();
        _bitmap = new WriteableBitmap(new PixelSize(width, height), Dpi, PixelFormat.Bgra8888, AlphaFormat.Opaque);
        Source = _bitmap;
        return _bitmap;
    }
}
