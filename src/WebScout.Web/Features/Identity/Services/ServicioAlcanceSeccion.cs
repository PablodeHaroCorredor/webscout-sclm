using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Infrastructure.Persistence;

namespace WebScout.Web.Features.Identity.Services;

public interface IServicioAlcanceSeccion
{
    /// <summary>Alcance del usuario actual, leído de base de datos en cada llamada (sin caché).</summary>
    Task<AlcanceSeccion> ObtenerAlcanceAsync();
}

/// <summary>
/// Resuelve el usuario autenticado tanto en un circuito Blazor como en un endpoint HTTP.
/// </summary>
public interface IUsuarioActual
{
    Task<Guid?> ObtenerIdAsync();
}

public sealed class UsuarioActual(
    IHttpContextAccessor httpContextAccessor,
    AuthenticationStateProvider? authenticationStateProvider = null) : IUsuarioActual
{
    public async Task<Guid?> ObtenerIdAsync()
    {
        ClaimsPrincipal? usuario = null;

        if (authenticationStateProvider is not null)
        {
            try
            {
                usuario = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
            }
            catch (InvalidOperationException)
            {
                // Fuera de un circuito o render de componentes (p. ej. endpoint mínimo): se usa HttpContext
            }
        }

        if (usuario?.Identity?.IsAuthenticated != true)
        {
            usuario = httpContextAccessor.HttpContext?.User;
        }

        var id = usuario?.Identity?.IsAuthenticated == true
            ? usuario.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;

        return Guid.TryParse(id, out var guid) ? guid : null;
    }
}

/// <summary>
/// Calcula el alcance por sección consultando en cada operación la cuenta, sus roles y su asignación vigente.
/// No usa los claims de la cookie para el alcance: así una revocación se aplica en la siguiente petición (PA02).
/// </summary>
public sealed class ServicioAlcanceSeccion(WebScoutDbContext db, IUsuarioActual usuarioActual) : IServicioAlcanceSeccion
{
    private static readonly string[] RolesGlobales = [RolesScout.Coordinacion, RolesScout.Administracion, RolesScout.SuperAdmin];

    public async Task<AlcanceSeccion> ObtenerAlcanceAsync()
    {
        var usuarioId = await usuarioActual.ObtenerIdAsync();
        if (usuarioId is null)
        {
            return AlcanceSeccion.Ninguno;
        }

        // El filtro global de tenant impide resolver cuentas de otro grupo
        var activo = await db.Users.AsNoTracking()
            .Where(u => u.Id == usuarioId.Value)
            .Select(u => (bool?)u.Activo)
            .FirstOrDefaultAsync();

        if (activo != true)
        {
            return AlcanceSeccion.Ninguno;
        }

        var roles = await db.UserRoles.AsNoTracking()
            .Where(ur => ur.UserId == usuarioId.Value)
            .Join(db.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .ToListAsync();

        if (roles.Any(r => r is not null && RolesGlobales.Contains(r)))
        {
            return AlcanceSeccion.Global;
        }

        if (!roles.Contains(RolesScout.Scouter))
        {
            return AlcanceSeccion.Ninguno;
        }

        var asignacion = await db.AsignacionesSeccion.AsNoTracking()
            .Where(a => a.UsuarioId == usuarioId.Value && a.RevocadaUtc == null)
            .Select(a => new { a.Rama, a.Funcion })
            .FirstOrDefaultAsync();

        return asignacion is null || asignacion.Rama == RamaScout.Ninguna
            ? AlcanceSeccion.Ninguno
            : AlcanceSeccion.DeSeccion(asignacion.Rama, asignacion.Funcion);
    }
}
