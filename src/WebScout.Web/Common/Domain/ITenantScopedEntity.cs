namespace WebScout.Web.Common.Domain;

/// <summary>
/// Contrato fundamental para todas las entidades de negocio multitenant.
/// Garantiza la presencia de OrganizationId para el aislamiento lógico mediante Global Query Filters.
/// </summary>
public interface ITenantScopedEntity
{
    Guid OrganizationId { get; set; }
}
