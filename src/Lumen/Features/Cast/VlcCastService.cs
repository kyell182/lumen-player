using LibVLCSharp.Shared;
using Lumen.Features.Playback;
using Lumen.Features.Playback.Engine;

namespace Lumen.Features.Cast;

/// <summary>Zoekt Chromecasts op het netwerk via VLC en stuurt de video ernaartoe.</summary>
public sealed class VlcCastService : ICastService, IDisposable
{
    private readonly VlcEngine _engine;
    private readonly IMediaReloader _reloader;
    private readonly object _gate = new();
    private readonly Dictionary<Guid, (CastDevice Device, RendererItem Item)> _found = new();
    private RendererDiscoverer? _discoverer;

    public VlcCastService(VlcEngine engine, IMediaReloader reloader)
    {
        _engine = engine;
        _reloader = reloader;
    }

    public event EventHandler? DevicesChanged;

    public CastDevice? Current { get; private set; }

    public IReadOnlyList<CastDevice> Devices
    {
        get
        {
            lock (_gate) return _found.Values.Select(f => f.Device).OrderBy(d => d.Name).ToList();
        }
    }

    public void StartDiscovery()
    {
        if (_discoverer is not null) return;

        var name = _engine.LibVlc.RendererList.Select(r => r.Name).FirstOrDefault(n => !string.IsNullOrEmpty(n));
        if (name is null) return; // deze VLC-versie kan niet casten

        _discoverer = new RendererDiscoverer(_engine.LibVlc, name);
        _discoverer.ItemAdded += (_, e) => Add(e.RendererItem);
        _discoverer.ItemDeleted += (_, e) => Remove(e.RendererItem);
        _discoverer.Start();
    }

    public void CastTo(CastDevice? device)
    {
        RendererItem? item = null;
        if (device is not null)
        {
            lock (_gate)
            {
                if (_found.TryGetValue(device.Id, out var found)) item = found.Item;
            }
        }

        _engine.Player.SetRenderer(item);
        Current = item is null ? null : device;

        // VLC wisselt pas van uitvoer als het bestand opnieuw geopend wordt.
        _reloader.Reload();
    }

    public void Dispose()
    {
        _discoverer?.Stop();
        _discoverer?.Dispose();
    }

    private void Add(RendererItem item)
    {
        lock (_gate)
        {
            var device = new CastDevice(Guid.NewGuid(), item.Name, item.Type);
            _found[device.Id] = (device, item);
        }

        DevicesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Remove(RendererItem item)
    {
        lock (_gate)
        {
            var match = _found.FirstOrDefault(f => f.Value.Item.Name == item.Name && f.Value.Item.Type == item.Type);
            if (match.Value.Device is not null) _found.Remove(match.Key);
        }

        DevicesChanged?.Invoke(this, EventArgs.Empty);
    }
}
