using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Administration.Domain;
using WebScout.Web.Features.Admissions.Domain;
using WebScout.Web.Features.Audit.Domain;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Inscriptions.Domain;
using WebScout.Web.Features.Orders.Domain;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Outbox.Domain;
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
    private readonly IServicioCifrado _cifrador;

    /// <param name="cifrador">
    /// En la aplicación lo inyecta DI con la clave de configuración (ver <see cref="CifradorDatosSensibles.DesdeConfiguracion"/>).
    /// Si se omite (pruebas con datos sintéticos) se usa la clave pública de desarrollo.
    /// EF Core construye el modelo una sola vez por proceso, así que los conversores quedan ligados al cifrador
    /// del primer contexto creado: debe ser el mismo singleton en toda la aplicación.
    /// </param>
    public WebScoutDbContext(
        DbContextOptions<WebScoutDbContext> options,
        ITenantContext tenantContext,
        IServicioCifrado? cifrador = null)
        : base(options)
    {
        _tenantContext = tenantContext;
        _cifrador = cifrador ?? CifradorDatosSensibles.ParaDesarrollo();
    }

    public DbSet<GrupoScout> GruposScout => Set<GrupoScout>();
    public DbSet<ActividadScout> Actividades => Set<ActividadScout>();
    public DbSet<Educando> Educandos => Set<Educando>();
    public DbSet<FichaMedica> FichasMedicas => Set<FichaMedica>();
    public DbSet<InscripcionActividad> Inscripciones => Set<InscripcionActividad>();
    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();
    public DbSet<MensajeOutbox> MensajesOutbox => Set<MensajeOutbox>();
    public DbSet<SolicitudAdmision> SolicitudesAdmision => Set<SolicitudAdmision>();
    public DbSet<MensajeContacto> MensajesContacto => Set<MensajeContacto>();
    public DbSet<ConfiguracionGrupo> ConfiguracionesGrupo => Set<ConfiguracionGrupo>();
    public DbSet<ProductoMaterial> ProductosMaterial => Set<ProductoMaterial>();
    public DbSet<PedidoMaterial> PedidosMaterial => Set<PedidoMaterial>();
    public DbSet<LineaPedidoMaterial> LineasPedidoMaterial => Set<LineaPedidoMaterial>();

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
        var cifrador = _cifrador;
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

        // 9. Configuración del Buzón de Salida Transaccional (Outbox - ADR07 / PA11)
        modelBuilder.Entity<MensajeOutbox>(b =>
        {
            b.ToTable("MensajesOutbox");
            b.HasKey(m => m.Id);
            b.Property(m => m.TipoEvento).HasMaxLength(100).IsRequired();
            b.Property(m => m.ClaveDeduplicacion).HasMaxLength(150).IsRequired();
            b.Property(m => m.DestinatarioEmail).HasMaxLength(256).IsRequired();
            b.Property(m => m.DestinatarioNombre).HasMaxLength(150).IsRequired();
            b.Property(m => m.Asunto).HasMaxLength(250).IsRequired();
            b.Property(m => m.ReplyToEmail).HasMaxLength(256);
            b.Property(m => m.RemitenteNombre).HasMaxLength(150);
            b.Property(m => m.CuerpoHtml).IsRequired();
            b.Property(m => m.CuerpoTextoPlano).IsRequired();
            b.Property(m => m.ErrorUltimoReintento).HasMaxLength(2000);

            b.HasIndex(m => new { m.OrganizacionId, m.ClaveDeduplicacion }).IsUnique();
            b.HasIndex(m => new { m.Estado, m.ProximoReintentoUtc });
            b.HasIndex(m => m.FechaCreacionUtc);
        });

        // 10. Configuración de Solicitudes de Admisión (RF12 / BL03)
        modelBuilder.Entity<SolicitudAdmision>(b =>
        {
            b.ToTable("SolicitudesAdmision");
            b.HasKey(s => s.Id);
            b.Property(s => s.NombreEducando).HasMaxLength(100).IsRequired();
            b.Property(s => s.ApellidosEducando).HasMaxLength(150).IsRequired();
            b.Property(s => s.NombreTutor).HasMaxLength(150).IsRequired();
            b.Property(s => s.RelacionTutor).HasMaxLength(100).IsRequired();
            b.Property(s => s.EmailTutor).HasMaxLength(256).IsRequired();
            b.Property(s => s.TelefonoTutor).HasMaxLength(30).IsRequired();
            b.Property(s => s.Observaciones).HasMaxLength(2000);
            b.Property(s => s.NotasKraal).HasMaxLength(2000);

            b.HasIndex(s => new { s.OrganizacionId, s.Estado });
            b.HasIndex(s => new { s.OrganizacionId, s.RamaSugerida });
            b.HasIndex(s => new { s.OrganizacionId, s.FechaSolicitudUtc });
        });

        // 11. Configuración de Mensajes de Contacto (RF11 / BL03)
        modelBuilder.Entity<MensajeContacto>(b =>
        {
            b.ToTable("MensajesContacto");
            b.HasKey(m => m.Id);
            b.Property(m => m.NombreRemitente).HasMaxLength(150).IsRequired();
            b.Property(m => m.EmailRemitente).HasMaxLength(256).IsRequired();
            b.Property(m => m.TelefonoRemitente).HasMaxLength(30);
            b.Property(m => m.Asunto).HasMaxLength(200).IsRequired();
            b.Property(m => m.Mensaje).HasMaxLength(4000).IsRequired();
            b.Property(m => m.NotasKraal).HasMaxLength(2000);

            b.HasIndex(m => new { m.OrganizacionId, m.Leido });
            b.HasIndex(m => new { m.OrganizacionId, m.FechaEnvioUtc });
        });

        // 12. Configuración de Ajustes de Grupo por Áreas (RF64 / BL10)
        modelBuilder.Entity<ConfiguracionGrupo>(b =>
        {
            b.ToTable("ConfiguracionesGrupo");
            b.HasKey(c => c.Id);
            b.Property(c => c.Cif).HasMaxLength(30);
            b.Property(c => c.DireccionBase).HasMaxLength(300);
            b.Property(c => c.TelefonoContacto).HasMaxLength(50);
            b.Property(c => c.EmailOficial).HasMaxLength(256);
            b.Property(c => c.RondaSolarActiva).HasMaxLength(50).IsRequired();
            b.Property(c => c.IbanCuentaBancaria).HasMaxLength(50);
            b.Property(c => c.TitularCuenta).HasMaxLength(150);
            b.Property(c => c.ConceptoCuotaSugerido).HasMaxLength(200);
            b.Property(c => c.MensajeBienvenidaAdmisiones).HasMaxLength(2000);
            b.Property(c => c.EstadoUltimoBackup).HasMaxLength(200);

            b.HasIndex(c => c.OrganizacionId).IsUnique();
        });

        // 13. Configuración de Material Scout y Pedidos Core 1.29 (RF48-RF54 / BL07)
        modelBuilder.Entity<ProductoMaterial>(b =>
        {
            b.ToTable("ProductosMaterial");
            b.HasKey(p => p.Id);
            b.Property(p => p.Nombre).HasMaxLength(150).IsRequired();
            b.Property(p => p.Descripcion).HasMaxLength(1000);
            b.Property(p => p.Categoria).HasMaxLength(100);
            b.Property(p => p.ImagenUrl).HasMaxLength(500);
            b.Property(p => p.TallasDisponiblesCsv).HasMaxLength(200);
            b.Property(p => p.PrecioUnitario).HasPrecision(10, 2);

            b.HasIndex(p => new { p.OrganizacionId, p.Activo });
            b.HasIndex(p => new { p.OrganizacionId, p.Categoria });
        });

        modelBuilder.Entity<PedidoMaterial>(b =>
        {
            b.ToTable("PedidosMaterial");
            b.HasKey(p => p.Id);
            b.Property(p => p.CodigoPedido).HasMaxLength(50).IsRequired();
            b.Property(p => p.SolicitanteNombre).HasMaxLength(150).IsRequired();
            b.Property(p => p.SolicitanteEmail).HasMaxLength(256).IsRequired();
            b.Property(p => p.SolicitanteTelefono).HasMaxLength(50);
            b.Property(p => p.EducandoNombre).HasMaxLength(150);
            b.Property(p => p.RamaEducativa).HasMaxLength(50);
            b.Property(p => p.MotivoEstado).HasMaxLength(1000);
            b.Property(p => p.ObservacionesFamilia).HasMaxLength(1000);
            b.Property(p => p.NotasInternasKraal).HasMaxLength(1000);
            b.Property(p => p.ModificadoPor).HasMaxLength(150);
            b.Property(p => p.ComprobantePagoRuta).HasMaxLength(500);
            b.Property(p => p.ComprobantePagoNombreOriginal).HasMaxLength(255);
            b.Property(p => p.TotalEstimado).HasPrecision(10, 2);

            b.HasIndex(p => new { p.OrganizacionId, p.CodigoPedido }).IsUnique();
            b.HasIndex(p => new { p.OrganizacionId, p.UsuarioId });
            b.HasIndex(p => new { p.OrganizacionId, p.Estado });
            b.HasIndex(p => new { p.OrganizacionId, p.FechaSolicitudUtc });

            b.HasMany(p => p.Lineas)
                .WithOne(l => l.Pedido)
                .HasForeignKey(l => l.PedidoMaterialId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LineaPedidoMaterial>(b =>
        {
            b.ToTable("LineasPedidoMaterial");
            b.HasKey(l => l.Id);
            b.Property(l => l.ProductoNombre).HasMaxLength(150).IsRequired();
            b.Property(l => l.Talla).HasMaxLength(50);
            b.Property(l => l.PrecioUnitario).HasPrecision(10, 2);

            b.HasIndex(l => new { l.OrganizacionId, l.PedidoMaterialId });
            b.HasIndex(l => new { l.OrganizacionId, l.ProductoMaterialId });

            b.HasOne(l => l.Producto)
                .WithMany()
                .HasForeignKey(l => l.ProductoMaterialId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // 14. Aplicación automática de Global Query Filters en todas las entidades IEntidadOrganizacion
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
