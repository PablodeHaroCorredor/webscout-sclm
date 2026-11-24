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
    public const string SuperAdmin = "SuperAdmin";

    public static readonly string[] Todos = [Familia, Scouter, Coordinacion, Administracion, SuperAdmin];

    // Listas de roles por área. Se usan a la vez en [Authorize] de las páginas y en los AuthorizeView del menú,
    // para que nunca se muestre un enlace que la página va a denegar.

    /// <summary>Área "Gestión Kraal": mesa de inscripciones, censo, actividades, dossier, admisiones y pedidos.</summary>
    public const string AreaKraal = Scouter + "," + Coordinacion + "," + Administracion;

    /// <summary>Ajustes propios de un grupo scout (no aplican a la federación).</summary>
    public const string AreaAjustesGrupo = Coordinacion + "," + Administracion;

    /// <summary>Supervisión: correos, auditoría y diagnóstico. Incluye a la federación.</summary>
    public const string AreaSupervision = Coordinacion + "," + Administracion + "," + SuperAdmin;
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
    public const string SuperAdmin = RolesScout.SuperAdmin;

    public static readonly string[] All = RolesScout.Todos;
}
