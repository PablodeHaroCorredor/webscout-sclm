namespace WebScout.Web.Common.Domain;

/// <summary>
/// Entidad raíz que representa a un grupo scout federado en SCLM (un Tenant).
/// </summary>
public class ScoutTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identificador único para el subdominio (ej: "lestonnac" en "lestonnac.scoutsclm.org").
    /// </summary>
    public required string Slug { get; set; }

    /// <summary>
    /// Nombre oficial del grupo scout (ej: "Grupo Scout Lestonnac 330").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Número identificador del grupo federado en SCLM.
    /// </summary>
    public int GroupNumber { get; set; }

    /// <summary>
    /// Dominio personalizado opcional con CNAME (ej: "www.scoutslestonnac.es").
    /// </summary>
    public string? CustomDomain { get; set; }

    /// <summary>
    /// Lema scout del grupo (ej: "Siempre Listos").
    /// </summary>
    public string? Motto { get; set; }

    /// <summary>
    /// Correo electrónico institucional de coordinación.
    /// </summary>
    public required string ContactEmail { get; set; }

    /// <summary>
    /// Indica si el portal del grupo está habilitado.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Identidad de pañoleta oficial.
    /// </summary>
    public PanoletaConfig Panoleta { get; set; } = PanoletaConfig.Lestonnac330;

    /// <summary>
    /// Paleta visual validada con WCAG 2.2 AA.
    /// </summary>
    public PaletteConfig Palette { get; set; } = PaletteConfig.DefaultLestonnac;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
