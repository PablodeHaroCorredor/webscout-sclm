using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Identity.Domain;

namespace WebScout.Web.Infrastructure.Tenancy;

/// <summary>
/// Implementación con ámbito (scoped) de ITenantContext para la solicitud HTTP actual o circuito Blazor.
/// Resuelve automáticamente el inquilino a partir de:
/// 1. Claims del usuario autenticado (OrganizacionId)
/// 2. Parámetro query ?tenant= o Cookie .WebScout.Tenant
/// 3. Subdominio del Host
/// 4. Fallback de desarrollo a Grupo Scout Lestonnac 330
/// </summary>
public class TenantContext : ITenantContext
{
    public static readonly Guid OrgLestonnac = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid OrgSanVicente = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly AuthenticationStateProvider? _authStateProvider;

    private Guid _currentTenantId = Guid.Empty;
    private string _tenantSlug = string.Empty;
    private string _groupName = string.Empty;
    private int _groupNumber;
    private PaletteConfig? _palette;
    private bool _isResolved;
    private bool _isSuperAdmin;

    public TenantContext()
    {
    }

    public TenantContext(
        IHttpContextAccessor? httpContextAccessor = null,
        AuthenticationStateProvider? authStateProvider = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _authStateProvider = authStateProvider;
    }

    public Guid CurrentTenantId
    {
        get
        {
            EnsureResolved();
            return _currentTenantId;
        }
    }

    public string TenantSlug
    {
        get
        {
            EnsureResolved();
            return _tenantSlug;
        }
    }

    public string GroupName
    {
        get
        {
            EnsureResolved();
            return _groupName;
        }
    }

    public int GroupNumber
    {
        get
        {
            EnsureResolved();
            return _groupNumber;
        }
    }

    public PaletteConfig? Palette
    {
        get
        {
            EnsureResolved();
            return _palette;
        }
    }

    public bool IsResolved
    {
        get
        {
            EnsureResolved();
            return _isResolved;
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            EnsureResolved();
            return _isSuperAdmin;
        }
    }

    private void EnsureResolved()
    {
        if (_isResolved) return;

        // 1. Resolver por Subdominio en la URL (Host)
        var http = _httpContextAccessor?.HttpContext;
        if (http != null)
        {
            if (http.Request.Headers.TryGetValue("X-Tenant-Slug", out var headerSlug) && !string.IsNullOrWhiteSpace(headerSlug))
            {
                AplicarPresetPorSlug(headerSlug.ToString().Trim().ToLowerInvariant());
                if (_isResolved)
                {
                    VerificarSuperAdmin();
                    return;
                }
            }

            var host = http.Request.Host.Host;

            // Subdominio en localhost: ej. lestonnac.localhost o sanvicente.localhost
            if (host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase))
            {
                var parts = host.Split('.');
                if (parts.Length > 1 && !parts[0].Equals("www", StringComparison.OrdinalIgnoreCase))
                {
                    AplicarPresetPorSlug(parts[0].ToLowerInvariant());
                    if (_isResolved)
                    {
                        VerificarSuperAdmin();
                        return;
                    }
                }
            }

            // Subdominio en dominio de producción: ej. lestonnac.scoutsclm.org
            if (host.EndsWith("scoutsclm.org", StringComparison.OrdinalIgnoreCase))
            {
                var prefix = host.Substring(0, host.Length - "scoutsclm.org".Length).TrimEnd('.');
                if (!string.IsNullOrWhiteSpace(prefix) && !prefix.Equals("www", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = prefix.Split('.');
                    AplicarPresetPorSlug(parts[^1].ToLowerInvariant());
                    if (_isResolved)
                    {
                        VerificarSuperAdmin();
                        return;
                    }
                }
            }
        }

        // 2. Verificar rol SuperAdmin o claims de usuario
        VerificarSuperAdmin();

        // 3. Si se accede a bare domain (localhost o scoutsclm.org sin subdominio), es la federación SCLM
        if (http != null)
        {
            SetTenant(Guid.Empty, "sclm", "Scouts de Castilla-La Mancha (SCLM)", 0);
        }
    }

    private void VerificarSuperAdmin()
    {
        var httpUser = _httpContextAccessor?.HttpContext?.User;
        if (httpUser?.Identity?.IsAuthenticated == true && httpUser.IsInRole(RolesScout.SuperAdmin))
        {
            _isSuperAdmin = true;
            return;
        }

        if (_authStateProvider != null)
        {
            try
            {
                var authTask = _authStateProvider.GetAuthenticationStateAsync();
                if (authTask.IsCompletedSuccessfully)
                {
                    var user = authTask.Result.User;
                    if (user?.Identity?.IsAuthenticated == true)
                    {
                        if (user.IsInRole(RolesScout.SuperAdmin))
                        {
                            _isSuperAdmin = true;
                        }
                    }
                }
            }
            catch
            {
                // Silencioso si el circuito aún no está inicializado
            }
        }
    }

    public void SetSuperAdmin(bool isSuperAdmin = true)
    {
        _isSuperAdmin = isSuperAdmin;
    }

    private void AplicarPresetPorSlug(string slug)
    {
        if (slug.Equals("sanvicente", StringComparison.OrdinalIgnoreCase))
        {
            SetTenant(OrgSanVicente, "sanvicente", "Grupo Scout San Vicente 412", 412, new PaletteConfig
            {
                PrimaryHex = "#0D47A1",
                SecondaryHex = "#D32F2F",
                AccentHex = "#F57C00",
                TextOnPrimaryHex = "#FFFFFF",
                TextOnSecondaryHex = "#FFFFFF",
                SurfaceHeaderHex = "#0D47A1"
            });
        }
        else if (slug.Equals("lestonnac", StringComparison.OrdinalIgnoreCase))
        {
            SetTenant(OrgLestonnac, "lestonnac", "Grupo Scout Lestonnac 330", 330, PaletteConfig.DefaultLestonnac);
        }
        else if (slug.Equals("admin", StringComparison.OrdinalIgnoreCase) || slug.Equals("sclm", StringComparison.OrdinalIgnoreCase))
        {
            SetTenant(Guid.Empty, "sclm", "Scouts de Castilla-La Mancha (SCLM)", 0, null);
        }
    }

    public void SetTenant(Guid tenantId, string slug, string groupName, int groupNumber, PaletteConfig? palette = null)
    {
        _currentTenantId = tenantId;
        _tenantSlug = slug;
        _groupName = groupName;
        _groupNumber = groupNumber;
        _palette = palette;
        _isResolved = true;
    }

    public void Clear()
    {
        _currentTenantId = Guid.Empty;
        _tenantSlug = string.Empty;
        _groupName = string.Empty;
        _groupNumber = 0;
        _palette = null;
        _isResolved = false;
    }
}
