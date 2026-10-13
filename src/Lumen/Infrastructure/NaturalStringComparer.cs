namespace Lumen.Infrastructure;

/// <summary>Sorteert zoals een mens: "Aflevering 2" komt vóór "Aflevering 10".</summary>
public sealed class NaturalStringComparer : IComparer<string>
{
    public static readonly NaturalStringComparer Instance = new();

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        int i = 0, j = 0;
        while (i < x.Length && j < y.Length)
        {
            int result = char.IsDigit(x[i]) && char.IsDigit(y[j])
                ? CompareNumberChunk(x, ref i, y, ref j)
                : CompareCharacter(x[i++], y[j++]);

            if (result != 0) return result;
        }

        return (x.Length - i).CompareTo(y.Length - j);
    }

    private static int CompareCharacter(char a, char b) =>
        char.ToUpperInvariant(a).CompareTo(char.ToUpperInvariant(b));

    private static int CompareNumberChunk(string x, ref int i, string y, ref int j)
    {
        var numberX = ReadDigits(x, ref i).TrimStart('0');
        var numberY = ReadDigits(y, ref j).TrimStart('0');

        return numberX.Length != numberY.Length
            ? numberX.Length.CompareTo(numberY.Length)
            : numberX.CompareTo(numberY, StringComparison.Ordinal);
    }

    private static ReadOnlySpan<char> ReadDigits(string text, ref int index)
    {
        int start = index;
        while (index < text.Length && char.IsDigit(text[index])) index++;
        return text.AsSpan(start, index - start);
    }
}
