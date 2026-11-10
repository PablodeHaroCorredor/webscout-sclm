namespace WebScout.Web.Common.Domain;

/// <summary>
/// Modo de visualización para mesas de gestión y listas de registros.
/// Permite conmutar entre vista tabular tradicional y tarjetas compactas táctiles.
/// </summary>
public enum ModoVisualizacion
{
    /// <summary>
    /// Automático según el breakpoint: Tabla en escritorio/tablet (md+), Tarjetas en móvil (xs/sm).
    /// </summary>
    Auto,

    /// <summary>
    /// Forzar visualización en tabla densa con cabecera fija.
    /// </summary>
    Tabla,

    /// <summary>
    /// Forzar visualización en tarjetas compactas ergonómicas.
    /// </summary>
    Tarjetas
}
