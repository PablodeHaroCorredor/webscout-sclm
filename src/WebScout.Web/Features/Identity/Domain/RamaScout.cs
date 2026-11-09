namespace WebScout.Web.Features.Identity.Domain;

/// <summary>
/// Secciones o Ramas pedagógicas oficiales en Scouts de Castilla-La Mancha (SCLM).
/// </summary>
public enum RamaScout
{
    Ninguna = 0,
    Castores = 1,
    Manada = 2,
    Tropa = 3,
    Escultas = 4,
    Clan = 5
}

/// <summary>
/// Alias retrocompatible de transición para RamaScout.
/// </summary>
public enum ScoutBranch
{
    None = 0,
    Castores = 1,
    Manada = 2,
    Tropa = 3,
    Escultas = 4,
    Clan = 5
}

public static class RamaScoutExtensions
{
    public static string ObtenerNombre(this RamaScout rama) => rama switch
    {
        RamaScout.Castores => "Castores",
        RamaScout.Manada => "Manada",
        RamaScout.Tropa => "Tropa",
        RamaScout.Escultas => "Escultas",
        RamaScout.Clan => "Clan",
        _ => "Sin Rama"
    };
}
