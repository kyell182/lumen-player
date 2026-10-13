namespace Lumen.Features.Playback.Tracks;

/// <summary>Zet taalcodes en Engelse taalnamen ("eng", "English", "nl") om naar Nederlandse namen.</summary>
public static class LanguageNames
{
    public const string Dutch = "Nederlands";
    public const string English = "Engels";

    private static readonly Dictionary<string, string> Names = Build(
        (Dutch, new[] { "nl", "nld", "dut", "dutch", "nederlands", "vlaams", "flemish" }),
        (English, new[] { "en", "eng", "english", "engels" }),
        ("Frans", new[] { "fr", "fre", "fra", "french", "francais", "français" }),
        ("Duits", new[] { "de", "ger", "deu", "german", "deutsch" }),
        ("Spaans", new[] { "es", "spa", "spanish", "espanol", "español", "castilian", "latino" }),
        ("Italiaans", new[] { "it", "ita", "italian", "italiano" }),
        ("Portugees", new[] { "pt", "por", "portuguese", "brazilian" }),
        ("Chinees", new[] { "zh", "chi", "zho", "chinese", "mandarin", "cantonese" }),
        ("Japans", new[] { "ja", "jpn", "japanese" }),
        ("Koreaans", new[] { "ko", "kor", "korean" }),
        ("Thais", new[] { "th", "tha", "thai" }),
        ("Russisch", new[] { "ru", "rus", "russian" }),
        ("Arabisch", new[] { "ar", "ara", "arabic" }),
        ("Turks", new[] { "tr", "tur", "turkish" }),
        ("Pools", new[] { "pl", "pol", "polish" }),
        ("Zweeds", new[] { "sv", "swe", "swedish" }),
        ("Deens", new[] { "da", "dan", "danish" }),
        ("Noors", new[] { "no", "nor", "nob", "norwegian" }),
        ("Fins", new[] { "fi", "fin", "finnish" }),
        ("Grieks", new[] { "el", "gre", "ell", "greek" }),
        ("Tsjechisch", new[] { "cs", "cze", "ces", "czech" }),
        ("Hongaars", new[] { "hu", "hun", "hungarian" }),
        ("Roemeens", new[] { "ro", "rum", "ron", "romanian" }),
        ("Hebreeuws", new[] { "he", "heb", "hebrew" }),
        ("Indonesisch", new[] { "id", "ind", "indonesian" }),
        ("Vietnamees", new[] { "vi", "vie", "vietnamese" }),
        ("Hindi", new[] { "hin", "hindi" }));

    /// <summary>De Nederlandse naam, of null als het geen bekende taal is.</summary>
    public static string? ToDutch(string? token) =>
        token is not null && Names.TryGetValue(token.Trim(), out var name) ? name : null;

    /// <summary>De eerste herkende taal in een rij woorden.</summary>
    public static string? FirstIn(IEnumerable<string> tokens) =>
        tokens.Select(ToDutch).FirstOrDefault(n => n is not null);

    private static Dictionary<string, string> Build(params (string Name, string[] Aliases)[] languages)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, aliases) in languages)
        {
            foreach (var alias in aliases) map[alias] = name;
        }

        return map;
    }
}
