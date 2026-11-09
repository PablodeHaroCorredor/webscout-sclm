namespace WebScout.Web.Infrastructure.Tenancy;

/// <summary>
/// Proporciona acceso al contexto del grupo scout activo resuelto para la solicitud actual.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Identificador único de la organización scout (tenant).
    /// </summary>
    Guid CurrentTenantId { get; }

    /// <summary>
    /// Slug identificativo del grupo (ej. "lestonnac", "sanvicente").
    /// </summary>
    string TenantSlug { get; }

    /// <summary>
    /// Nombre oficial del grupo scout.
    /// </summary>
    string GroupName { get; }

    /// <summary>
    /// Número de grupo federado en SCLM (ej. 330, 412).
    /// </summary>
    int GroupNumber { get; }

    /// <summary>
    /// Indica si la petición fue resuelta a un grupo activo válido.
    /// </summary>
    bool IsResolved { get; }

    /// <summary>
    /// Indica si el contexto actual corresponde a un usuario con rol SuperAdmin de la federación.
    /// </summary>
    bool IsSuperAdmin { get; }
}
