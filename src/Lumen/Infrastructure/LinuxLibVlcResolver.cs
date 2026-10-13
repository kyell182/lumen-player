using System.Reflection;
using System.Runtime.InteropServices;

namespace Lumen.Infrastructure;

/// <summary>
/// Op Linux staat libVLC vaak alleen als "libvlc.so.5" geïnstalleerd (zonder -dev pakket).
/// Deze resolver valt daarop terug zodat de app zonder extra pakketten start.
/// </summary>
internal static class LinuxLibVlcResolver
{
    private static readonly Dictionary<string, string> Fallbacks = new()
    {
        ["libvlc"] = "libvlc.so.5",
        ["libvlccore"] = "libvlccore.so.9",
    };

    public static void Register()
    {
        if (!OperatingSystem.IsLinux()) return;

        try
        {
            NativeLibrary.SetDllImportResolver(typeof(LibVLCSharp.Shared.Core).Assembly, Resolve);
        }
        catch (InvalidOperationException)
        {
            // LibVLCSharp heeft zelf al een resolver ingesteld; die gebruiken we dan.
        }
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (NativeLibrary.TryLoad(libraryName, assembly, searchPath, out var handle)) return handle;

        var key = libraryName.Split('.')[0];
        return Fallbacks.TryGetValue(key, out var fallback) && NativeLibrary.TryLoad(fallback, out handle)
            ? handle
            : IntPtr.Zero;
    }
}
