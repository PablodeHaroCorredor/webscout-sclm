using WebScout.Web.Features.Admissions.Domain;
using WebScout.Web.Features.Identity.Domain;

namespace WebScout.Web.Features.Admissions.Services;

public record RegistrarSolicitudAdmisionDto(
    string NombreEducando,
    string ApellidosEducando,
    DateOnly FechaNacimiento,
    string NombreTutor,
    string RelacionTutor,
    string EmailTutor,
    string TelefonoTutor,
    string? Observaciones,
    bool ConsentimientoPrivacidad,
    bool ConsentimientoComunicaciones
);

public record RegistrarMensajeContactoDto(
    string NombreRemitente,
    string EmailRemitente,
    string? TelefonoRemitente,
    string Asunto,
    string Mensaje,
    bool ConsentimientoPrivacidad
);

/// <summary>
/// Contrato del servicio de admisión de educandos y buzón de contacto público (RF10 a RF14 / BL03).
/// </summary>
public interface IServicioAdmisiones
{
    Task<SolicitudAdmision> RegistrarSolicitudAdmisionAsync(
        RegistrarSolicitudAdmisionDto dto,
        CancellationToken cancellationToken = default);

    Task<MensajeContacto> RegistrarMensajeContactoAsync(
        RegistrarMensajeContactoDto dto,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SolicitudAdmision>> ObtenerSolicitudesAsync(
        EstadoSolicitudAdmision? filtroEstado = null,
        RamaScout? filtroRama = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MensajeContacto>> ObtenerMensajesContactoAsync(
        bool? soloNoLeidos = null,
        CancellationToken cancellationToken = default);

    Task CambiarEstadoSolicitudAsync(
        Guid solicitudId,
        EstadoSolicitudAdmision nuevoEstado,
        string? notasKraal,
        Guid scouterId,
        CancellationToken cancellationToken = default);

    Task MarcarMensajeLeidoAsync(
        Guid mensajeId,
        bool leido,
        CancellationToken cancellationToken = default);
}
