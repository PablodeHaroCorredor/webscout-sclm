using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebScout.Web.Features.Administration.Domain;
using WebScout.Web.Features.Audit.Services;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;

namespace WebScout.Web.Features.Administration.Services;

public class ServicioConfiguracionGrupo : IServicioConfiguracionGrupo
{
    private readonly WebScoutDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IServicioAuditoria _auditoria;
    private readonly ILogger<ServicioConfiguracionGrupo> _logger;

    public ServicioConfiguracionGrupo(
        WebScoutDbContext dbContext,
        ITenantContext tenantContext,
        IServicioAuditoria auditoria,
        ILogger<ServicioConfiguracionGrupo> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<ConfiguracionGrupo> ObtenerConfiguracionAsync(CancellationToken cancellationToken = default)
    {
        var config = await _dbContext.Set<ConfiguracionGrupo>()
            .FirstOrDefaultAsync(c => c.OrganizacionId == _tenantContext.CurrentTenantId, cancellationToken);

        if (config == null)
        {
            config = new ConfiguracionGrupo
            {
                OrganizacionId = _tenantContext.CurrentTenantId,
                EmailOficial = _tenantContext.GroupName,
                FechaModificacionUtc = DateTime.UtcNow
            };

            _dbContext.Set<ConfiguracionGrupo>().Add(config);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return config;
    }

    public async Task<ConfiguracionGrupo> GuardarAreaInstitucionalAsync(
        AjustesInstitucionalesDto dto,
        CancellationToken cancellationToken = default)
    {
        var config = await ObtenerConfiguracionAsync(cancellationToken);

        config.Cif = dto.Cif?.Trim();
        config.DireccionBase = dto.DireccionBase?.Trim();
        config.TelefonoContacto = dto.TelefonoContacto?.Trim();
        config.EmailOficial = dto.EmailOficial?.Trim();
        config.RondaSolarActiva = string.IsNullOrWhiteSpace(dto.RondaSolarActiva) ? "2026/2027" : dto.RondaSolarActiva.Trim();
        config.FechaInicioRonda = dto.FechaInicioRonda;
        config.FechaFinRonda = dto.FechaFinRonda;
        config.FechaModificacionUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditoria.RegistrarAsync(
            accion: "ActualizacionAjustesInstitucionales",
            tipoRecurso: nameof(ConfiguracionGrupo),
            recursoId: config.Id.ToString(),
            detalles: $"Ronda solar: {config.RondaSolarActiva}, CIF: {config.Cif ?? "No especificado"}",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Ajustes institucionales guardados para el tenant {TenantId}", _tenantContext.CurrentTenantId);
        return config;
    }

    public async Task<ConfiguracionGrupo> GuardarAreaEconomicaAsync(
        AjustesEconomicosDto dto,
        CancellationToken cancellationToken = default)
    {
        var config = await ObtenerConfiguracionAsync(cancellationToken);

        config.IbanCuentaBancaria = dto.IbanCuentaBancaria?.Trim();
        config.TitularCuenta = dto.TitularCuenta?.Trim();
        config.ConceptoCuotaSugerido = dto.ConceptoCuotaSugerido?.Trim();
        config.FechaModificacionUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditoria.RegistrarAsync(
            accion: "ActualizacionAjustesEconomicos",
            tipoRecurso: nameof(ConfiguracionGrupo),
            recursoId: config.Id.ToString(),
            detalles: $"IBAN actualizado: {(string.IsNullOrEmpty(config.IbanCuentaBancaria) ? "No especificado" : "IBAN configurado (enmascarado)")}",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Ajustes económicos guardados para el tenant {TenantId}", _tenantContext.CurrentTenantId);
        return config;
    }

    public async Task<ConfiguracionGrupo> GuardarAreaAdmisionesAsync(
        AjustesAdmisionesDto dto,
        CancellationToken cancellationToken = default)
    {
        var config = await ObtenerConfiguracionAsync(cancellationToken);

        config.AdmisionesAbiertas = dto.AdmisionesAbiertas;
        config.MensajeBienvenidaAdmisiones = dto.MensajeBienvenidaAdmisiones?.Trim();
        config.FechaModificacionUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditoria.RegistrarAsync(
            accion: "ActualizacionAjustesAdmisiones",
            tipoRecurso: nameof(ConfiguracionGrupo),
            recursoId: config.Id.ToString(),
            detalles: $"Admisiones abiertas: {config.AdmisionesAbiertas}",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Ajustes de admisiones guardados para el tenant {TenantId}", _tenantContext.CurrentTenantId);
        return config;
    }

    public async Task<ConfiguracionGrupo> RegistrarSimulacionBackupAsync(
        string? estadoDetalle = null,
        CancellationToken cancellationToken = default)
    {
        var config = await ObtenerConfiguracionAsync(cancellationToken);

        config.FechaUltimoBackupUtc = DateTime.UtcNow;
        config.EstadoUltimoBackup = estadoDetalle ?? $"Verificado con éxito ({DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC)";
        config.FechaModificacionUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditoria.RegistrarAsync(
            accion: "VerificacionCopiaSeguridad",
            tipoRecurso: nameof(ConfiguracionGrupo),
            recursoId: config.Id.ToString(),
            detalles: $"Simulación de backup PA19: {config.EstadoUltimoBackup}",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Simulación de respaldo PA19 registrada para el tenant {TenantId}", _tenantContext.CurrentTenantId);
        return config;
    }
}
