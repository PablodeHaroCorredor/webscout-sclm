using System.ComponentModel.DataAnnotations.Schema;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Identity.Domain;

namespace WebScout.Web.Features.Activities.Domain;

/// <summary>
/// Convocatoria de actividad, salida o campamento scout por ramas pedagógicas.
/// Implementa IEntidadOrganizacion para aislamiento estricto multi-inquilino.
/// </summary>
public class ActividadScout : IEntidadOrganizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrganizacionId { get; set; }

    public required string Titulo { get; set; }

    public string? Descripcion { get; set; }

    public TipoActividad Tipo { get; set; } = TipoActividad.Salida;

    public EstadoActividad Estado { get; set; } = EstadoActividad.Borrador;

    /// <summary>
    /// Ramas o secciones a las que va dirigida la actividad.
    /// Si la lista está vacía, se considera convocada para todo el Grupo Scout.
    /// </summary>
    public List<RamaScout> RamasDestinatarias { get; set; } = [];

    public required string Lugar { get; set; }

    public string? PuntoEncuentroSalida { get; set; }

    public string? PuntoEncuentroRegreso { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public DateTime? FechaLimiteInscripcion { get; set; }

    /// <summary>
    /// Cuota por educando en euros (con decimales para céntimos, ej: 15.50 €). 0 si es gratuita.
    /// </summary>
    public decimal Precio { get; set; } = 0.00m;

    public string? IbanCuentaBancaria { get; set; }

    public string? ConceptoPagoSugerido { get; set; }

    /// <summary>
    /// Lista o recomendaciones de material para la mochila (saco, cantimplora, botas, etc.).
    /// </summary>
    public string? MaterialRequerido { get; set; }

    /// <summary>
    /// Indicaciones sanitarias específicas (vacunación adicional, prevención garrapatas, etc.).
    /// </summary>
    public string? ObservacionesSanitarias { get; set; }

    public Guid? CreadoPorUsuarioId { get; set; }

    public UsuarioScout? CreadoPorUsuario { get; set; }

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    public DateTime? FechaModificacionUtc { get; set; }

    /// <summary>
    /// Determina si la actividad aplica a una rama específica o al grupo completo.
    /// </summary>
    public bool AplicaARama(RamaScout rama) =>
        RamasDestinatarias.Count == 0 || RamasDestinatarias.Contains(rama);

    /// <summary>
    /// Indica si la actividad está en periodo abierto de inscripción.
    /// </summary>
    [NotMapped]
    public bool AdmiteInscripciones =>
        Estado == EstadoActividad.Publicada &&
        (!FechaLimiteInscripcion.HasValue || FechaLimiteInscripcion.Value.Date >= DateTime.UtcNow.Date);

    #region Retrocompatibilidad y transiciones
    [NotMapped]
    public bool Publicada
    {
        get => Estado == EstadoActividad.Publicada;
        set => Estado = value ? EstadoActividad.Publicada : EstadoActividad.Borrador;
    }

    [NotMapped]
    public int PrecioEnCentimos
    {
        get => (int)Math.Round(Precio * 100);
        set => Precio = value / 100m;
    }

    [NotMapped]
    public Guid OrganizationId
    {
        get => OrganizacionId;
        set => OrganizacionId = value;
    }

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
    #endregion
}

/// <summary>
/// Alias retrocompatible de transición para código existente.
/// </summary>
public class ScoutActivity : ActividadScout
{
}
