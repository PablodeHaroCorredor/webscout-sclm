namespace WebScout.Web.Common.Domain;

/// <summary>
/// Objeto de Valor (Value Object) que define la identidad cromática y heráldica
/// de la pañoleta scout oficial de un grupo federado en SCLM.
/// </summary>
public record PanoletaConfig
{
    public required string BaseColorHex { get; init; }
    public required string BorderColorHex { get; init; }
    public string? CentralStripeColorHex { get; init; }
    public string? SecondaryBorderColorHex { get; init; }
    public bool HasEmblem { get; init; }
    public string? ShieldUrl { get; init; }

    /// <summary>
    /// Configuración de referencia del Grupo Scout Lestonnac 330 (Tenant Piloto).
    /// </summary>
    public static PanoletaConfig Lestonnac330 => new()
    {
        BaseColorHex = "#1B5E20",        // Verde bosque
        BorderColorHex = "#FBC02D",      // Amarillo dorado
        CentralStripeColorHex = "#FFFFFF", // Cinta central blanca
        HasEmblem = true,
        ShieldUrl = "/tenants/lestonnac/escudo-l330.webp"
    };

    /// <summary>
    /// Configuración de referencia del Grupo Scout San Vicente 412 (Tenant de contraste).
    /// </summary>
    public static PanoletaConfig SanVicente412 => new()
    {
        BaseColorHex = "#0D47A1",        // Azul marino
        BorderColorHex = "#D32F2F",      // Rojo
        CentralStripeColorHex = null,
        HasEmblem = false
    };
}
