namespace WebScout.Web.Features.Activities.Domain;

/// <summary>
/// Tipología pedagógica y organizativa de la convocatoria scout.
/// </summary>
public enum TipoActividad
{
    /// <summary>
    /// Actividad habitual o extraordinaria en el local o base habitual (duración de horas).
    /// </summary>
    Reunion = 1,

    /// <summary>
    /// Excursión o salida de día completo sin pernocta fuera del local.
    /// </summary>
    Salida = 2,

    /// <summary>
    /// Salida de fin de semana con 1 o 2 pernoctas (albergue, tienda o vivac).
    /// </summary>
    Acampada = 3,

    /// <summary>
    /// Campamento estacional de larga duración (Navidad, Semana Santa o Verano).
    /// </summary>
    Campamento = 4
}
