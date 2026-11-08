using System.Globalization;

namespace WebScout.Web.Common.Wcag;

/// <summary>
/// Motor matemático de cálculo de luminancia y contraste según el estándar W3C WCAG 2.2.
/// Se utiliza para validar que las paletas de los grupos scouts cumplen el ratio mínimo AA (>= 4.5:1).
/// </summary>
public static class WcagContrastCalculator
{
    public const double MinimumNormalTextRatio = 4.5;
    public const double MinimumLargeTextRatio = 3.0;

    /// <summary>
    /// Calcula la luminancia relativa (L) de un color hexadecimal (#RRGGBB).
    /// L = 0.2126 * R + 0.7152 * G + 0.0722 * B
    /// </summary>
    public static double CalculateRelativeLuminance(string hexColor)
    {
        var (r, g, b) = ParseHexToRgb(hexColor);

        var rLinear = ToLinearRgb(r / 255.0);
        var gLinear = ToLinearRgb(g / 255.0);
        var bLinear = ToLinearRgb(b / 255.0);

        return (0.2126 * rLinear) + (0.7152 * gLinear) + (0.0722 * bLinear);
    }

    /// <summary>
    /// Calcula el ratio de contraste entre dos colores hexadecimales.
    /// Ratio = (L1 + 0.05) / (L2 + 0.05) donde L1 es el más claro.
    /// </summary>
    public static double CalculateContrastRatio(string hexColor1, string hexColor2)
    {
        var l1 = CalculateRelativeLuminance(hexColor1);
        var l2 = CalculateRelativeLuminance(hexColor2);

        var lighter = Math.Max(l1, l2);
        var darker = Math.Min(l1, l2);

        return (lighter + 0.05) / (darker + 0.05);
    }

    /// <summary>
    /// Verifica si el par de colores cumple el estándar WCAG 2.2 AA para texto normal (>= 4.5:1).
    /// </summary>
    public static bool MeetsWcagAaNormalText(string foregroundHex, string backgroundHex)
    {
        return CalculateContrastRatio(foregroundHex, backgroundHex) >= MinimumNormalTextRatio;
    }

    /// <summary>
    /// Elige automáticamente el color de texto más legible (#FFFFFF o #1A1A1A) para un fondo dado.
    /// </summary>
    public static string GetAccessibleTextColor(string backgroundHex)
    {
        var whiteRatio = CalculateContrastRatio("#FFFFFF", backgroundHex);
        var darkRatio = CalculateContrastRatio("#1A1A1A", backgroundHex);

        return whiteRatio >= darkRatio ? "#FFFFFF" : "#1A1A1A";
    }

    private static double ToLinearRgb(double channel)
    {
        return channel <= 0.04045
            ? channel / 12.92
            : Math.Pow((channel + 0.055) / 1.055, 2.4);
    }

    private static (int r, int g, int b) ParseHexToRgb(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            throw new ArgumentException("El color hexadecimal no puede estar vacío.", nameof(hex));

        var clean = hex.Trim().TrimStart('#');
        if (clean.Length != 6)
            throw new ArgumentException($"Formato hexadecimal inválido: '{hex}'. Se espera #RRGGBB.", nameof(hex));

        var r = int.Parse(clean[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var g = int.Parse(clean.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var b = int.Parse(clean.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        return (r, g, b);
    }
}
