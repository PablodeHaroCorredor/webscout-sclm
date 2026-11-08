using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Tenancy;

namespace WebScout.Web.Infrastructure.Persistence;

/// <summary>
/// Contexto de base de datos relacional PostgreSQL de la plataforma WebScout SCLM.
/// Integra ASP.NET Core Identity con aislamiento multitenant estricto (OrganizacionId).
/// Modelo de datos normalizado íntegramente en castellano.
/// </summary>
public class WebScoutDbContext : IdentityDbContext<UsuarioScout, RolScout, Guid>
{
    private readonly ITenantContext _tenantContext;

    public WebScoutDbContext(DbContextOptions<WebScoutDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<GrupoScout> GruposScout => Set<GrupoScout>();
    public DbSet<ActividadScout> Actividades => Set<ActividadScout>();
    public DbSet<Educando> Educandos => Set<Educando>();
    public DbSet<FichaMedica> FichasMedicas => Set<FichaMedica>();

    // Aliases retrocompatibles para consultas existentes
    public DbSet<GrupoScout> Tenants => GruposScout;
    public DbSet<ActividadScout> Activities => Actividades;
    public DbSet<Educando> Participants => Educandos;
    public DbSet<FichaMedica> HealthRecords => FichasMedicas;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Configuración de tablas de ASP.NET Core Identity
        base.OnModelCreating(modelBuilder);

        // 2. Configuración de la entidad GrupoScout (Tenant)
        modelBuilder.Entity<GrupoScout>(b =>
        {
            b.ToTable("GruposScout");
            b.HasKey(t => t.Id);
            b.HasIndex(t => t.Slug).IsUnique();
            b.Property(t => t.Slug).HasMaxLength(64).IsRequired();
            b.Property(t => t.Nombre).HasMaxLength(128).IsRequired();
            b.Property(t => t.EmailContacto).HasMaxLength(256).IsRequired();

            // Mapeo de Value Objects de identidad
            b.OwnsOne(t => t.Panoleta);
            b.OwnsOne(t => t.Paleta);
        });

        // 3. Configuración de UsuarioScout (Multitenant)
        modelBuilder.Entity<UsuarioScout>(b =>
        {
            b.Property(u => u.NombreCompleto).HasMaxLength(150).IsRequired();
            b.Property(u => u.Dni).HasMaxLength(20);
            b.HasIndex(u => new { u.OrganizacionId, u.NormalizedEmail });
        });

        // 4. Configuración de Actividades
        modelBuilder.Entity<ActividadScout>(b =>
        {
            b.ToTable("Actividades");
            b.HasKey(a => a.Id);
            b.Property(a => a.Titulo).HasMaxLength(150).IsRequired();
            b.Property(a => a.Lugar).HasMaxLength(200).IsRequired();
            b.HasIndex(a => new { a.OrganizacionId, a.FechaInicio });
        });

        // 5. Configuración de Educandos
        modelBuilder.Entity<Educando>(b =>
        {
            b.ToTable("Educandos");
            b.HasKey(p => p.Id);
            b.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
            b.Property(p => p.Apellidos).HasMaxLength(150).IsRequired();
            b.Property(p => p.Dni).HasMaxLength(20);
            b.Property(p => p.Sexo).HasMaxLength(20);

            b.HasIndex(p => new { p.OrganizacionId, p.TutorLegalId });
            b.HasIndex(p => new { p.OrganizacionId, p.RamaActual });
            b.HasIndex(p => new { p.OrganizacionId, p.Activo });

            b.HasOne(p => p.TutorLegal)
                .WithMany(u => u.Educandos)
                .HasForeignKey(p => p.TutorLegalId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(p => p.FichaMedica)
                .WithOne(h => h.Educando)
                .HasForeignKey<FichaMedica>(h => h.EducandoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 6. Configuración de Ficha Médica Anual
        modelBuilder.Entity<FichaMedica>(b =>
        {
            b.ToTable("FichasMedicas");
            b.HasKey(h => h.Id);
            b.Property(h => h.NumeroSeguridadSocial).HasMaxLength(50).IsRequired();
            b.Property(h => h.EntidadAseguradora).HasMaxLength(100);
            b.Property(h => h.TelefonoEmergenciaPrincipal).HasMaxLength(30).IsRequired();
            b.Property(h => h.NombreContactoEmergenciaPrincipal).HasMaxLength(100);
            b.Property(h => h.TelefonoEmergenciaSecundario).HasMaxLength(30);
            b.Property(h => h.NombreContactoEmergenciaSecundario).HasMaxLength(100);

            b.HasIndex(h => new { h.OrganizacionId, h.EducandoId }).IsUnique();
        });

        // 7. Aplicación automática de Global Query Filters en todas las entidades IEntidadOrganizacion
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IEntidadOrganizacion).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(WebScoutDbContext)
                    .GetMethod(nameof(ConfigureGlobalTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)?
                    .MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, [modelBuilder]);
            }
        }
    }

    private void ConfigureGlobalTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, IEntidadOrganizacion
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => e.OrganizacionId == _tenantContext.CurrentTenantId);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EstamparOrganizacionId();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        EstamparOrganizacionId();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EstamparOrganizacionId()
    {
        foreach (var entry in ChangeTracker.Entries<IEntidadOrganizacion>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.OrganizacionId == Guid.Empty)
                {
                    if (_tenantContext.CurrentTenantId == Guid.Empty)
                    {
                        throw new InvalidOperationException(
                            $"Operación bloqueada: No se puede persistir la entidad '{entry.Entity.GetType().Name}' sin una Organización activa en el contexto.");
                    }

                    entry.Entity.OrganizacionId = _tenantContext.CurrentTenantId;
                }
            }
        }
    }
}
