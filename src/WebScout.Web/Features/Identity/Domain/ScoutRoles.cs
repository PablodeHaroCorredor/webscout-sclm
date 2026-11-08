namespace WebScout.Web.Features.Identity.Domain;

/// <summary>
/// Constantes de roles normativos en WebScout SCLM.
/// </summary>
public static class ScoutRoles
{
    public const string Familia = "Familia";
    public const string Scouter = "Scouter";
    public const string Coordinacion = "Coordinacion";
    public const string Administracion = "Administracion";

    public static readonly IReadOnlyList<string> All = [Familia, Scouter, Coordinacion, Administracion];
}
