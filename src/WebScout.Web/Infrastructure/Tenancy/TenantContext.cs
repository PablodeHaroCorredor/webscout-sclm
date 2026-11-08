namespace WebScout.Web.Infrastructure.Tenancy;

/// <summary>
/// Implementación con ámbito (scoped) de ITenantContext para la solicitud HTTP actual.
/// </summary>
public class TenantContext : ITenantContext
{
    public Guid CurrentTenantId { get; private set; } = Guid.Empty;
    public string TenantSlug { get; private set; } = string.Empty;
    public string GroupName { get; private set; } = string.Empty;
    public int GroupNumber { get; private set; }
    public bool IsResolved { get; private set; }

    /// <summary>
    /// Establece el tenant activo una vez resuelto por el middleware.
    /// </summary>
    public void SetTenant(Guid tenantId, string slug, string groupName, int groupNumber)
    {
        CurrentTenantId = tenantId;
        TenantSlug = slug;
        GroupName = groupName;
        GroupNumber = groupNumber;
        IsResolved = true;
    }

    /// <summary>
    /// Limpia el contexto (por ejemplo, en rutas federativas de SCLM o fallos de resolución).
    /// </summary>
    public void Clear()
    {
        CurrentTenantId = Guid.Empty;
        TenantSlug = string.Empty;
        GroupName = string.Empty;
        GroupNumber = 0;
        IsResolved = false;
    }
}
