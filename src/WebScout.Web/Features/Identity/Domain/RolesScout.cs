namespace WebScout.Web.Features.Identity.Domain;

/// <summary>
/// Roles funcionales y de pertenencia del sistema WebScout SCLM.
/// </summary>
public static class RolesScout
{
    public const string Familia = "Familia";
    public const string Scouter = "Scouter";
    public const string Coordinacion = "Coordinacion";
    public const string Administracion = "Administracion";

    public static readonly string[] Todos = [Familia, Scouter, Coordinacion, Administracion];
}

/// <summary>
/// Alias retrocompatible de transición para RolesScout.
/// </summary>
public static class ScoutRoles
{
    public const string Familia = RolesScout.Familia;
    public const string Scouter = RolesScout.Scouter;
    public const string Coordinacion = RolesScout.Coordinacion;
    public const string Administracion = RolesScout.Administracion;

    public static readonly string[] All = RolesScout.Todos;
}
