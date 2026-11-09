namespace WebScout.Web.Common.Domain;

/// <summary>
/// Organización o Grupo Scout federado en Scouts de Castilla-La Mancha (SCLM).
/// Constituye la frontera estricta de aislamiento de datos (Tenant).
/// </summary>
public class GrupoScout
{
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador amigable para subdominio o ruta (ej. 'lestonnac', 'sanvicente').
    /// </summary>
    public required string Slug { get; set; }

    /// <summary>
    /// Nombre oficial del grupo (ej. 'Grupo Scout Lestonnac 330').
    /// </summary>
    public required string Nombre { get; set; }

    /// <summary>
    /// Número de grupo asignado por la asociación (ej. 330).
    /// </summary>
    public int NumeroGrupo { get; set; }

    /// <summary>
    /// Lema scout del grupo (ej. 'Siempre listos para servir').
    /// </summary>
    public string? Lema { get; set; }

    public required string EmailContacto { get; set; }

    public bool Activo { get; set; } = true;

    /// <summary>
    /// Paleta de colores corporativos del grupo conforme a WCAG 2.2 AA (Value Object).
    /// </summary>
    public PaletteConfig Paleta { get; set; } = null!;

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    // Propiedades puente retrocompatibles
    public string Name
    {
        get => Nombre;
        set => Nombre = value;
    }

    public int GroupNumber
    {
        get => NumeroGrupo;
        set => NumeroGrupo = value;
    }

    public string? Motto
    {
        get => Lema;
        set => Lema = value;
    }

    public string ContactEmail
    {
        get => EmailContacto;
        set => EmailContacto = value;
    }

    public bool IsActive
    {
        get => Activo;
        set => Activo = value;
    }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public PaletteConfig Palette
    {
        get => Paleta;
        set => Paleta = value;
    }
}

/// <summary>
/// Alias retrocompatible de transición para GrupoScout.
/// </summary>
public class ScoutTenant : GrupoScout
{
}
