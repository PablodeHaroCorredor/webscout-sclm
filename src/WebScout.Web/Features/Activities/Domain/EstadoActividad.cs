namespace WebScout.Web.Features.Activities.Domain;

/// <summary>
/// Ciclo de vida y disponibilidad de una actividad scout.
/// </summary>
public enum EstadoActividad
{
    /// <summary>
    /// En preparación por los Scouters / Kraal. No visible para las familias.
    /// </summary>
    Borrador = 1,

    /// <summary>
    /// Visible para las familias de las ramas correspondientes y abierta para consulta e inscripción.
    /// </summary>
    Publicada = 2,

    /// <summary>
    /// Plazo de inscripción finalizado o aforo completo. Sigue visible para las familias con inscritos.
    /// </summary>
    InscripcionesCerradas = 3,

    /// <summary>
    /// En curso (durante las fechas de realización).
    /// </summary>
    EnCurso = 4,

    /// <summary>
    /// Actividad finalizada satisfactoriamente.
    /// </summary>
    Finalizada = 5,

    /// <summary>
    /// Actividad cancelada por causas meteorológicas, logísticas u organizativas.
    /// </summary>
    Cancelada = 6
}
