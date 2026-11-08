namespace WebScout.Web.Common.Domain;

/// <summary>
/// Entidad de negocio multitenant para salidas y campamentos scouts.
/// Implementa ITenantScopedEntity para garantizar el aislamiento por OrganizationId.
/// </summary>
public class ScoutActivity : ITenantScopedEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Organización scout propietaria de la actividad (Tenant).
    /// </summary>
    public Guid OrganizationId { get; set; }

    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string Location { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal Price { get; set; } = 0.00m;
    public bool RequiresPaymentReceipt { get; set; } = false;

    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
