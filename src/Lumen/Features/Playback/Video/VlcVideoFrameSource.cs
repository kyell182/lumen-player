using System.Runtime.InteropServices;
using System.Text;
using LibVLCSharp.Shared;

namespace Lumen.Features.Playback.Video;

/// <summary>
/// Laat VLC elk frame in ons eigen geheugen decoderen (RV32 = BGRA) in plaats van in een eigen venster.
/// De callbacks draaien op de videothread van VLC.
/// </summary>
public sealed class VlcVideoFrameSource : IVideoFrameSource, IDisposable
{
    private const int BytesPerPixel = 4;
    private static readonly byte[] Rv32Chroma = Encoding.ASCII.GetBytes("RV32");

    private readonly object _gate = new();

    // Velden houden de delegates in leven zolang VLC ze kan aanroepen.
    private readonly MediaPlayer.LibVLCVideoFormatCb _formatCallback;
    private readonly MediaPlayer.LibVLCVideoCleanupCb _cleanupCallback;
    private readonly MediaPlayer.LibVLCVideoLockCb _lockCallback;
    private readonly MediaPlayer.LibVLCVideoDisplayCb _displayCallback;

    private IntPtr _decodeBuffer;
    private byte[] _frame = Array.Empty<byte>();
    private int _width;
    private int _height;
    private int _stride;
    private int _visibleWidth;
    private int _visibleHeight;

    public VlcVideoFrameSource(MediaPlayer player)
    {
        _formatCallback = OnFormat;
        _cleanupCallback = OnCleanup;
        _lockCallback = OnLock;
        _displayCallback = OnDisplay;

        player.SetVideoFormatCallbacks(_formatCallback, _cleanupCallback);
        player.SetVideoCallbacks(_lockCallback, null, _displayCallback);
    }

    public event EventHandler? FrameReady;

    public bool ReadLatestFrame(FrameReader reader)
    {
        lock (_gate)
        {
            if (_visibleWidth == 0 || _visibleHeight == 0) return false;
            reader(_frame, _visibleWidth, _visibleHeight, _stride);
            return true;
        }
    }

    /// <summary>
    /// De echte beeldgrootte. VLC decodeert soms enkele rijen extra (bv. 1088 i.p.v. 1080);
    /// die worden zo weggelaten.
    /// </summary>
    public void SetVisibleSize(int width, int height)
    {
        lock (_gate)
        {
            if (width <= 0 || height <= 0) return;
            _visibleWidth = Math.Min(width, _width);
            _visibleHeight = Math.Min(height, _height);
        }
    }

    public void Dispose() => FreeDecodeBuffer();

    private uint OnFormat(ref IntPtr opaque, IntPtr chroma, ref uint width, ref uint height, ref uint pitches, ref uint lines)
    {
        Marshal.Copy(Rv32Chroma, 0, chroma, Rv32Chroma.Length);

        var stride = (int)width * BytesPerPixel;
        pitches = (uint)stride;
        lines = height;

        FreeDecodeBuffer();
        _decodeBuffer = Marshal.AllocHGlobal(stride * (int)height);

        lock (_gate)
        {
            _width = (int)width;
            _height = (int)height;
            _stride = stride;
            _visibleWidth = (int)width;
            _visibleHeight = (int)height;
            _frame = new byte[stride * (int)height];
        }

        return 1; // één beeldbuffer
    }

    private void OnCleanup(ref IntPtr opaque)
    {
        FreeDecodeBuffer();
        lock (_gate)
        {
            _width = 0;
            _height = 0;
            _visibleWidth = 0;
            _visibleHeight = 0;
        }

        FrameReady?.Invoke(this, EventArgs.Empty);
    }

    private IntPtr OnLock(IntPtr opaque, IntPtr planes)
    {
        Marshal.WriteIntPtr(planes, _decodeBuffer);
        return IntPtr.Zero;
    }

    private void OnDisplay(IntPtr opaque, IntPtr picture)
    {
        lock (_gate)
        {
            if (_decodeBuffer == IntPtr.Zero || _frame.Length == 0) return;
            Marshal.Copy(_decodeBuffer, _frame, 0, _frame.Length);
        }

        FrameReady?.Invoke(this, EventArgs.Empty);
    }

    private void FreeDecodeBuffer()
    {
        lock (_gate)
        {
            if (_decodeBuffer == IntPtr.Zero) return;
            Marshal.FreeHGlobal(_decodeBuffer);
            _decodeBuffer = IntPtr.Zero;
        }
    }
}
