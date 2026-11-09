using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Identity.Domain;

namespace WebScout.Web.Features.Activities.Services;

/// <summary>
/// Contrato del servicio de gestión y publicación de actividades y campamentos scout por ramas.
/// Garantiza aislamiento estricto por inquilino y control de visibilidad según roles y ramas.
/// </summary>
public interface IServicioActividades
{
    /// <summary>
    /// Obtiene las actividades para el Kraal/Scouters de la organización activa, con filtros opcionales.
    /// </summary>
    Task<IReadOnlyList<ActividadScout>> ObtenerActividadesKraalAsync(
        EstadoActividad? estado = null,
        RamaScout? rama = null);

    /// <summary>
    /// Obtiene las actividades visibles para una familia, restringidas a las ramas donde tiene educandos inscritos.
    /// Incluye actividades generales del grupo completo.
    /// </summary>
    Task<IReadOnlyList<ActividadScout>> ObtenerActividadesParaFamiliaAsync(Guid tutorId);

    /// <summary>
    /// Obtiene una actividad por su identificador único dentro de la organización activa.
    /// </summary>
    Task<ActividadScout?> ObtenerPorIdAsync(Guid actividadId);

    /// <summary>
    /// Da de alta una nueva convocatoria de actividad o campamento.
    /// </summary>
    Task<ActividadScout> CrearActividadAsync(ActividadScout actividad, Guid? scouterId = null);

    /// <summary>
    /// Actualiza los datos de una actividad existente.
    /// </summary>
    Task<ActividadScout> ActualizarActividadAsync(ActividadScout actividad);

    /// <summary>
    /// Transiciona el estado de la actividad (ej. Publicar, Cerrar Inscripciones, Cancelar).
    /// </summary>
    Task CambiarEstadoActividadAsync(Guid actividadId, EstadoActividad nuevoEstado);

    /// <summary>
    /// Elimina una actividad (solo permitido si está en Borrador o Cancelada).
    /// </summary>
    Task EliminarActividadAsync(Guid actividadId);
}
