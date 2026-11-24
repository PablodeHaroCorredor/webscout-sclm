namespace WebScout.Web.Common.Domain;

/// <summary>
/// Conversión entre UTC (almacenamiento) y la hora peninsular española (interfaz).
/// No depende de la zona horaria del servidor: el mismo campamento se muestra y se edita
/// a la misma hora aunque el servidor esté configurado en UTC.
/// </summary>
public static class HoraEspana
{
    public static readonly TimeZoneInfo Zona = ResolverZona();

    /// <summary>Convierte una fecha almacenada en UTC a la hora de España. Las fechas sin tipo se tratan como UTC.</summary>
    public static DateTime AHoraEspana(this DateTime fecha)
    {
        var utc = fecha.Kind switch
        {
            DateTimeKind.Local => fecha.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(fecha, DateTimeKind.Utc),
            _ => fecha
        };

        return TimeZoneInfo.ConvertTimeFromUtc(utc, Zona);
    }

    /// <summary>Convierte una fecha y hora introducidas en hora de España a UTC para guardarla.</summary>
    public static DateTime AUtc(DateTime horaEspana) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(horaEspana, DateTimeKind.Unspecified), Zona);

    private static TimeZoneInfo ResolverZona()
    {
        foreach (var id in new[] { "Europe/Madrid", "Romance Standard Time" })
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zona))
            {
                return zona;
            }
        }

        return TimeZoneInfo.Local;
    }
}
