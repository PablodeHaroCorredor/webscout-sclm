using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebScout.Web.Features.Audit.Domain;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;

namespace WebScout.Web.Features.Audit.Services;

public interface IServicioAuditoria
{
    Task RegistrarAsync(
        string accion,
        string tipoRecurso,
        string? recursoId,
        string? detalles = null,
        CancellationToken cancellationToken = default);
}

public class ServicioAuditoria : IServicioAuditoria
{
    private readonly WebScoutDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ServicioAuditoria> _logger;

    public ServicioAuditoria(
        WebScoutDbContext dbContext,
        ITenantContext tenantContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ServicioAuditoria> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task RegistrarAsync(
        string accion,
        string tipoRecurso,
        string? recursoId,
        string? detalles = null,
        CancellationToken cancellationToken = default)
    {
        var http = _httpContextAccessor.HttpContext;
        var user = http?.User;

        Guid? usuarioId = null;
        string? emailUsuario = null;

        if (user?.Identity?.IsAuthenticated == true)
        {
            var idClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(idClaim, out var parsedId))
            {
                usuarioId = parsedId;
            }
            emailUsuario = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity.Name;
        }

        var ip = http?.Connection?.RemoteIpAddress?.ToString();

        var evento = new RegistroAuditoria
        {
            OrganizacionId = _tenantContext.CurrentTenantId,
            UsuarioId = usuarioId,
            EmailUsuario = emailUsuario,
            Accion = accion,
            TipoRecurso = tipoRecurso,
            RecursoId = recursoId,
            Detalles = detalles,
            DireccionIp = ip,
            FechaCreacionUtc = DateTime.UtcNow
        };

        try
        {
            _dbContext.RegistrosAuditoria.Add(evento);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[AUDIT] {Accion} sobre {TipoRecurso} ({RecursoId}) por '{Usuario}' [Tenant: {TenantId}, IP: {IP}]",
                accion, tipoRecurso, recursoId, emailUsuario ?? "Anónimo", evento.OrganizacionId, ip ?? "Desconocida");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al persistir registro de auditoría para la acción {Accion}", accion);
        }
    }
}
