using System.ComponentModel.DataAnnotations.Schema;

namespace WebScout.Web.Common.Domain;

/// <summary>
/// Convocatoria de actividad, salida o campamento scout.
/// Implementa IEntidadOrganizacion para aislamiento multitenant.
/// </summary>
public class ActividadScout : IEntidadOrganizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrganizacionId { get; set; }

    public required string Titulo { get; set; }

    public string? Descripcion { get; set; }

    public required string Lugar { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public DateTime? FechaLimiteInscripcion { get; set; }

    /// <summary>
    /// Cuota por educando en céntimos de euro (ej. 2500 = 25,00 €). 0 si es gratuita.
    /// </summary>
    public int PrecioEnCentimos { get; set; } = 0;

    public string? IbanCuentaBancaria { get; set; }

    public bool Publicada { get; set; } = false;

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public Guid OrganizationId
    {
        get => OrganizacionId;
        set => OrganizacionId = value;
    }
}

/// <summary>
/// Alias retrocompatible de transición para ActividadScout.
/// </summary>
public class ScoutActivity : ActividadScout
{
    [NotMapped]
    public string Title
    {
        get => Titulo;
        set => Titulo = value;
    }

    [NotMapped]
    public string Location
    {
        get => Lugar;
        set => Lugar = value;
    }

    [NotMapped]
    public DateTime StartDate
    {
        get => FechaInicio;
        set => FechaInicio = value;
    }

    [NotMapped]
    public DateTime EndDate
    {
        get => FechaFin;
        set => FechaFin = value;
    }
}
