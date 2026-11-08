using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WebScout.Web.Common.Domain;
using WebScout.Web.Infrastructure.Tenancy;

namespace WebScout.Web.Infrastructure.Persistence;

/// <summary>
/// Contexto de base de datos relacional PostgreSQL de la plataforma WebScout SCLM.
/// Aplica de forma automática Global Query Filters y estampado de OrganizationId para el aislamiento lógico de datos.
/// </summary>
public class WebScoutDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public WebScoutDbContext(DbContextOptions<WebScoutDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<ScoutTenant> Tenants => Set<ScoutTenant>();
    public DbSet<ScoutActivity> Activities => Set<ScoutActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Configuración de la entidad Tenant
        modelBuilder.Entity<ScoutTenant>(b =>
        {
            b.HasKey(t => t.Id);
            b.HasIndex(t => t.Slug).IsUnique();
            b.Property(t => t.Slug).HasMaxLength(64).IsRequired();
            b.Property(t => t.Name).HasMaxLength(128).IsRequired();
            b.Property(t => t.ContactEmail).HasMaxLength(256).IsRequired();

            // Mapeo de Value Objects de identidad
            b.OwnsOne(t => t.Panoleta);
            b.OwnsOne(t => t.Palette);
        });

        // 2. Configuración de Actividades
        modelBuilder.Entity<ScoutActivity>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.Title).HasMaxLength(150).IsRequired();
            b.Property(a => a.Location).HasMaxLength(200).IsRequired();
            b.HasIndex(a => new { a.OrganizationId, a.StartDate });
        });

        // 3. Aplicación automática de Global Query Filters en todas las entidades ITenantScopedEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScopedEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(WebScoutDbContext)
                    .GetMethod(nameof(ConfigureGlobalTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)?
                    .MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, [modelBuilder]);
            }
        }
    }

    private void ConfigureGlobalTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScopedEntity
    {
        // builder.Entity<TEntity>().HasQueryFilter(e => e.OrganizationId == _tenantContext.CurrentTenantId);
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => e.OrganizationId == _tenantContext.CurrentTenantId);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        StampTenantId();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        StampTenantId();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void StampTenantId()
    {
        foreach (var entry in ChangeTracker.Entries<ITenantScopedEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (_tenantContext.CurrentTenantId == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        $"Operación bloqueada: No se puede persistir la entidad '{entry.Entity.GetType().Name}' sin un Tenant activo en el contexto.");
                }

                entry.Entity.OrganizationId = _tenantContext.CurrentTenantId;
            }
        }
    }
}
