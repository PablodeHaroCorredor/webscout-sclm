using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Identity.Domain;

namespace WebScout.Web.Features.Admissions.Domain;

/// <summary>
/// Representa una solicitud de admisión o preinscripción para incorporar a un menor al grupo scout (RF12 / BL03).
/// Se gestiona como registro privado sin alta de cuenta automática en esta fase (RF10).
/// </summary>
public class SolicitudAdmision : IEntidadOrganizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identificador del grupo scout destinatario (aislamiento multitenant estricto).
    /// </summary>
    public Guid OrganizacionId { get; set; }

    public string NombreEducando { get; set; } = string.Empty;
    public string ApellidosEducando { get; set; } = string.Empty;
    public DateOnly FechaNacimiento { get; set; }

    /// <summary>
    /// Rama scout correspondiente según la edad pedagógica en la fecha de solicitud.
    /// </summary>
    public RamaScout RamaSugerida { get; set; }

    public string NombreTutor { get; set; } = string.Empty;
    public string RelacionTutor { get; set; } = "Madre/Padre/Tutor legal";
    public string EmailTutor { get; set; } = string.Empty;
    public string TelefonoTutor { get; set; } = string.Empty;

    public string? Observaciones { get; set; }

    /// <summary>
    /// Consentimiento explícito de tratamiento de datos personales conforme al RGPD (obligatorio, PA13).
    /// </summary>
    public bool ConsentimientoPrivacidad { get; set; }

    /// <summary>
    /// Consentimiento opcional para recibir noticias y avisos de eventos no esenciales.
    /// </summary>
    public bool ConsentimientoComunicaciones { get; set; }

    public EstadoSolicitudAdmision Estado { get; set; } = EstadoSolicitudAdmision.Pendiente;

    /// <summary>
    /// Notas internas registradas por la secretaría o el Kraal del grupo (RF13).
    /// </summary>
    public string? NotasKraal { get; set; }

    public DateTime FechaSolicitudUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaResolucionUtc { get; set; }
    public Guid? AtendidoPorUsuarioId { get; set; }

    public string NombreCompletoEducando => $"{NombreEducando} {ApellidosEducando}".Trim();

    /// <summary>
    /// Calcula la rama scout pedagógica según la edad cumplida en el año scout en curso.
    /// </summary>
    public static RamaScout CalcularRamaPedagogica(DateOnly fechaNacimiento, DateOnly? fechaReferencia = null)
    {
        var hoy = fechaReferencia ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var edad = hoy.Year - fechaNacimiento.Year;
        if (fechaNacimiento > hoy.AddYears(-edad)) edad--;

        return edad switch
        {
            < 6 => RamaScout.Castores,
            >= 6 and <= 8 => RamaScout.Castores,
            >= 9 and <= 11 => RamaScout.Manada,
            >= 12 and <= 14 => RamaScout.Tropa,
            >= 15 and <= 17 => RamaScout.Escultas,
            _ => RamaScout.Clan
        };
    }
}
