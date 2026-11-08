namespace WebScout.Web.Common.Domain;

/// <summary>
/// Contrato que implementan todas las entidades vinculadas a un Grupo Scout (Tenant).
/// Garantiza la presencia de OrganizacionId para el aislamiento estricto de datos mediante filtros globales.
/// </summary>
public interface IEntidadOrganizacion
{
    Guid OrganizacionId { get; set; }
}

/// <summary>
/// Alias retrocompatible de transición para IEntidadOrganizacion.
/// </summary>
public interface ITenantScopedEntity : IEntidadOrganizacion
{
    Guid OrganizationId
    {
        get => OrganizacionId;
        set => OrganizacionId = value;
    }
}
