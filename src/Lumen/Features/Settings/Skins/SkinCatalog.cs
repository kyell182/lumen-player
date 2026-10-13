namespace Lumen.Features.Settings.Skins;

/// <summary>Alle beschikbare skins. Een nieuwe skin toevoegen = één regel hieronder.</summary>
public static class SkinCatalog
{
    public static readonly Skin Coral = Skin.From("Koraal", "#FF6B4A", "#FF8466", "#E9563A", "#FF8A4C", "#FF4D6D");

    public static readonly IReadOnlyList<Skin> All = new[]
    {
        Coral,
        Skin.From("Oceaan", "#3B82F6", "#5B9BFF", "#2563EB", "#38BDF8", "#6366F1"),
        Skin.From("Smaragd", "#10B981", "#34D399", "#059669", "#22C55E", "#14B8A6"),
        Skin.From("Violet", "#8B5CF6", "#A78BFA", "#7C3AED", "#A855F7", "#EC4899"),
        Skin.From("Roos", "#EC4899", "#F472B6", "#DB2777", "#F472B6", "#8B5CF6"),
        Skin.From("Zonsondergang", "#F59E0B", "#FBBF24", "#D97706", "#F59E0B", "#EF4444"),
        Skin.From("IJs", "#06B6D4", "#22D3EE", "#0891B2", "#67E8F9", "#3B82F6"),
        Skin.From("Grafiet", "#94A3B8", "#CBD5E1", "#64748B", "#94A3B8", "#475569"),
    };

    public static Skin ByName(string? name) =>
        All.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)) ?? Coral;
}
