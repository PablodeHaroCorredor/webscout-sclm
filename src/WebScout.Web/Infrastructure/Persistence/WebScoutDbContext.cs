using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Audit.Domain;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Inscriptions.Domain;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Security;
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
    public DbSet<InscripcionActividad> Inscripciones => Set<InscripcionActividad>();
    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();

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
            b.Property(a => a.Descripcion).HasMaxLength(2000);
            b.Property(a => a.Lugar).HasMaxLength(200).IsRequired();
            b.Property(a => a.PuntoEncuentroSalida).HasMaxLength(250);
            b.Property(a => a.PuntoEncuentroRegreso).HasMaxLength(250);
            b.Property(a => a.IbanCuentaBancaria).HasMaxLength(50);
            b.Property(a => a.ConceptoPagoSugerido).HasMaxLength(150);
            b.Property(a => a.MaterialRequerido).HasMaxLength(2000);
            b.Property(a => a.ObservacionesSanitarias).HasMaxLength(1000);
            b.Property(a => a.Precio).HasPrecision(10, 2);

            var ramasComparer = new ValueComparer<List<RamaScout>>(
                (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList());

            b.Property(a => a.RamasDestinatarias)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<RamaScout>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<RamaScout>()
                )
                .Metadata.SetValueComparer(ramasComparer);

            b.HasIndex(a => new { a.OrganizacionId, a.FechaInicio });
            b.HasIndex(a => new { a.OrganizacionId, a.Estado });

            b.HasOne(a => a.CreadoPorUsuario)
                .WithMany()
                .HasForeignKey(a => a.CreadoPorUsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
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

        // 6. Configuración de Ficha Médica Anual (Cifrado RGPD Art. 9/32)
        var cifrador = new CifradorDatosSensibles();
        var conversorCifradoNullable = new ValueConverter<string?, string?>(
            v => cifrador.Cifrar(v),
            v => cifrador.Descifrar(v));

        var conversorCifradoRequerido = new ValueConverter<string, string>(
            v => cifrador.Cifrar(v)!,
            v => cifrador.Descifrar(v) ?? string.Empty);

        modelBuilder.Entity<FichaMedica>(b =>
        {
            b.ToTable("FichasMedicas");
            b.HasKey(h => h.Id);
            b.Property(h => h.NumeroSeguridadSocial)
                .HasMaxLength(255)
                .IsRequired()
                .HasConversion(conversorCifradoRequerido);

            b.Property(h => h.EntidadAseguradora).HasMaxLength(100);
            b.Property(h => h.TelefonoEmergenciaPrincipal).HasMaxLength(30).IsRequired();
            b.Property(h => h.NombreContactoEmergenciaPrincipal).HasMaxLength(100);
            b.Property(h => h.TelefonoEmergenciaSecundario).HasMaxLength(30);
            b.Property(h => h.NombreContactoEmergenciaSecundario).HasMaxLength(100);

            // Campos sanitarios sensibles protegidos con AES-256-GCM
            b.Property(h => h.AlergiasAlimentarias).HasConversion(conversorCifradoNullable);
            b.Property(h => h.AlergiasMedicamentosas).HasConversion(conversorCifradoNullable);
            b.Property(h => h.AlergiasAmbientales).HasConversion(conversorCifradoNullable);
            b.Property(h => h.RegimenAlimenticio).HasConversion(conversorCifradoNullable);
            b.Property(h => h.EnfermedadesCronicas).HasConversion(conversorCifradoNullable);
            b.Property(h => h.MedicacionActiva).HasConversion(conversorCifradoNullable);
            b.Property(h => h.ObservacionesAdicionales).HasConversion(conversorCifradoNullable);

            b.HasIndex(h => new { h.OrganizacionId, h.EducandoId }).IsUnique();
        });

        // 7. Configuración de Inscripciones a Actividades
        modelBuilder.Entity<InscripcionActividad>(b =>
        {
            b.ToTable("Inscripciones");
            b.HasKey(i => i.Id);
            b.Property(i => i.ImporteEuros).HasPrecision(10, 2);
            b.Property(i => i.NombreArchivoJustificante).HasMaxLength(255);
            b.Property(i => i.RutaArchivoJustificante).HasMaxLength(500);
            b.Property(i => i.ObservacionesFamilia).HasMaxLength(1000);
            b.Property(i => i.NotasKraal).HasMaxLength(1000);
            b.Property(i => i.ObservacionesAsistencia).HasMaxLength(500);

            b.HasIndex(i => new { i.OrganizacionId, i.ActividadId, i.EducandoId }).IsUnique();
            b.HasIndex(i => new { i.OrganizacionId, i.TutorLegalId });
            b.HasIndex(i => new { i.OrganizacionId, i.Estado });

            b.HasOne(i => i.Actividad)
                .WithMany()
                .HasForeignKey(i => i.ActividadId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(i => i.Educando)
                .WithMany()
                .HasForeignKey(i => i.EducandoId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(i => i.TutorLegal)
                .WithMany()
                .HasForeignKey(i => i.TutorLegalId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(i => i.RevisadoPorUsuario)
                .WithMany()
                .HasForeignKey(i => i.RevisadoPorUsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // 8. Configuración de Registros de Auditoría Inmutable (PA06 / RF40)
        modelBuilder.Entity<RegistroAuditoria>(b =>
        {
            b.ToTable("RegistrosAuditoria");
            b.HasKey(a => a.Id);
            b.Property(a => a.Accion).HasMaxLength(100).IsRequired();
            b.Property(a => a.TipoRecurso).HasMaxLength(100).IsRequired();
            b.Property(a => a.RecursoId).HasMaxLength(100);
            b.Property(a => a.EmailUsuario).HasMaxLength(256);
            b.Property(a => a.DireccionIp).HasMaxLength(50);
            b.Property(a => a.Detalles).HasMaxLength(2000);

            b.HasIndex(a => new { a.OrganizacionId, a.FechaCreacionUtc });
            b.HasIndex(a => new { a.OrganizacionId, a.Accion });
        });

        // 8. Aplicación automática de Global Query Filters en todas las entidades IEntidadOrganizacion
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
            .HasQueryFilter(e => _tenantContext.IsSuperAdmin || e.OrganizacionId == _tenantContext.CurrentTenantId);
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
                    if (_tenantContext.CurrentTenantId != Guid.Empty)
                    {
                        entry.Entity.OrganizacionId = _tenantContext.CurrentTenantId;
                    }
                    else if (_tenantContext.IsSuperAdmin || entry.Entity is UsuarioScout)
                    {
                        // Permitido para administración federativa central (SuperAdmin / desarrolladores sin grupo fijado)
                        continue;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Operación bloqueada: No se puede persistir la entidad '{entry.Entity.GetType().Name}' sin una Organización activa en el contexto.");
                    }
                }
            }
        }
    }
}
