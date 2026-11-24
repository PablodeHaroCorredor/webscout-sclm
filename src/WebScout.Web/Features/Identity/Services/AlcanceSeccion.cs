using WebScout.Web.Features.Identity.Domain;

namespace WebScout.Web.Features.Identity.Services;

/// <summary>
/// Lo que el usuario actual puede ver y gestionar del Kraal, calculado en servidor a partir de sus roles
/// y de su asignación de sección vigentes en base de datos.
/// </summary>
/// <param name="EsGlobal">Coordinación, Administración o SuperAdmin: todas las secciones del grupo.</param>
/// <param name="Rama">Sección asignada a un Scouter o Coordinador de sección.</param>
/// <param name="Funcion">Función dentro de esa sección.</param>
public sealed record AlcanceSeccion(bool EsGlobal, RamaScout? Rama, FuncionSeccion? Funcion)
{
    public static readonly AlcanceSeccion Ninguno = new(false, null, null);
    public static readonly AlcanceSeccion Global = new(true, null, null);

    public static AlcanceSeccion DeSeccion(RamaScout rama, FuncionSeccion funcion) => new(false, rama, funcion);

    /// <summary>Tiene acceso a alguna sección del Kraal.</summary>
    public bool TieneAccesoKraal => EsGlobal || Rama is not null;

    /// <summary>Puede consultar educandos e inscripciones de la rama (censo, revisión, dossier).</summary>
    public bool PuedeVer(RamaScout rama) =>
        EsGlobal || (Rama is not null && rama != RamaScout.Ninguna && Rama == rama);

    /// <summary>Puede gestionar a los educandos de la rama (reservado al coordinador de esa sección).</summary>
    public bool PuedeGestionar(RamaScout rama) =>
        EsGlobal || (PuedeVer(rama) && Funcion == FuncionSeccion.Coordinador);

    /// <summary>
    /// Rama a la que debe limitarse una consulta: la pedida si es global, siempre la propia si no lo es.
    /// Devuelve null para «todas» (solo alcance global).
    /// </summary>
    public RamaScout? RamaDeConsulta(RamaScout? solicitada)
    {
        if (EsGlobal)
        {
            return solicitada is null or RamaScout.Ninguna ? null : solicitada;
        }

        return Rama;
    }
}
