namespace WebScout.Web.Infrastructure.Tenancy;

/// <summary>
/// Identidad del portal federativo de Scouts de Castilla-La Mancha (SCLM), que se sirve
/// en el dominio raíz y en los subdominios reservados <c>admin</c> y <c>sclm</c>.
/// No corresponde a ningún grupo de <c>GruposScout</c>: su <c>OrganizacionId</c> es <see cref="Guid.Empty"/>.
/// </summary>
public static class PortalFederativo
{
    public const string Slug = "sclm";
    public const string Nombre = "Scouts de Castilla-La Mancha (SCLM)";

    /// <summary>
    /// Indica si el slug corresponde al portal federativo y no a un grupo scout.
    /// </summary>
    public static bool EsSlugFederativo(string slug) =>
        slug.Equals("admin", StringComparison.OrdinalIgnoreCase)
        || slug.Equals(Slug, StringComparison.OrdinalIgnoreCase);
}
