namespace Lumen.Features.Convert;

/// <summary>
/// Een doelformaat voor converteren. <see cref="StreamOutput"/> is de VLC-"sout"-keten,
/// waarin {dst} vervangen wordt door het doelbestand.
/// </summary>
public sealed record ConversionProfile(string Name, string Extension, string StreamOutput, bool AudioOnly = false)
{
    public override string ToString() => Name;

    public string BuildStreamOutput(string destination) => StreamOutput.Replace("{dst}", Quote(destination));

    // VLC leest waarden tussen enkele aanhalingstekens letterlijk; anders tussen dubbele met escapes.
    private static string Quote(string path) => path.Contains('\'')
        ? "\"" + path.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""
        : $"'{path}'";
}
