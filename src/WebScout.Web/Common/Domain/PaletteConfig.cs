namespace WebScout.Web.Common.Domain;

/// <summary>
/// Tokens cromáticos validados para la interfaz web del grupo scout.
/// </summary>
public record PaletteConfig
{
    public required string PrimaryHex { get; init; }
    public required string SecondaryHex { get; init; }
    public required string AccentHex { get; init; }
    public required string TextOnPrimaryHex { get; init; }
    public required string TextOnSecondaryHex { get; init; }
    public required string SurfaceHeaderHex { get; init; }

    /// <summary>
    /// Paleta por defecto basada en Lestonnac 330.
    /// </summary>
    public static PaletteConfig DefaultLestonnac => new()
    {
        PrimaryHex = "#1B5E20",
        SecondaryHex = "#FBC02D",
        AccentHex = "#D32F2F",
        TextOnPrimaryHex = "#FFFFFF",
        TextOnSecondaryHex = "#1A1A1A",
        SurfaceHeaderHex = "#1B5E20"
    };
}
