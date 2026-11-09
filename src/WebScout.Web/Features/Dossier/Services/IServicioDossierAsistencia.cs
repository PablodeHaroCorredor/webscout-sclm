using WebScout.Web.Features.Dossier.Domain;

namespace WebScout.Web.Features.Dossier.Services;

/// <summary>
/// Contrato del servicio de generación del dossier de campamento y control de asistencia en tiempo real.
/// </summary>
public interface IServicioDossierAsistencia
{
    /// <summary>
    /// Genera y compila el dossier operativo completo de una actividad (fichero sanitario, intendencia, contactos y balance).
    /// </summary>
    Task<DossierOperativoActividad> ObtenerDossierActividadAsync(Guid actividadId);

    /// <summary>
    /// Registra el check-in de salida en el punto de encuentro / autobús para un educando.
    /// </summary>
    Task RegistrarCheckInSalidaAsync(Guid inscripcionId, bool presente);

    /// <summary>
    /// Registra el check-in de regreso y entrega del menor a su familia.
    /// </summary>
    Task RegistrarCheckInRegresoAsync(Guid inscripcionId, bool presente);

    /// <summary>
    /// Registra observaciones o incidencias de asistencia para una inscripción concreta.
    /// </summary>
    Task ActualizarObservacionAsistenciaAsync(Guid inscripcionId, string? observacion);

    /// <summary>
    /// Exporta los datos consolidados de asistencia en formato CSV con codificación UTF-8 compatible con Excel.
    /// </summary>
    Task<string> GenerarCsvAsistenciaAsync(Guid actividadId);
}
