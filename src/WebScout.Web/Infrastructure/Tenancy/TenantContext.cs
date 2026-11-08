using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

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
    private bool _isResolved;

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

    public bool IsResolved
    {
        get
        {
            EnsureResolved();
            return _isResolved;
        }
    }

    private void EnsureResolved()
    {
        if (_isResolved) return;

        // 1. Intentar resolver por usuario autenticado (Identity Claims)
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
                        var orgClaim = user.FindFirst("OrganizacionId")?.Value;
                        if (Guid.TryParse(orgClaim, out var orgId) && orgId != Guid.Empty)
                        {
                            if (orgId == OrgSanVicente)
                            {
                                SetTenant(OrgSanVicente, "sanvicente", "Grupo Scout San Vicente 412", 412);
                                return;
                            }
                            else
                            {
                                SetTenant(OrgLestonnac, "lestonnac", "Grupo Scout Lestonnac 330", 330);
                                return;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Silencioso si el circuito aún no está montado
            }
        }

        // 2. Intentar resolver por HttpContext (Query, Cookie, Subdominio)
        var http = _httpContextAccessor?.HttpContext;
        if (http != null)
        {
            // Query string ?tenant=xxx
            if (http.Request.Query.TryGetValue("tenant", out var qSlug) && !string.IsNullOrWhiteSpace(qSlug))
            {
                var slug = qSlug.ToString().Trim().ToLowerInvariant();
                AplicarPresetPorSlug(slug);
                if (_isResolved) return;
            }

            // Cookie .WebScout.Tenant
            if (http.Request.Cookies.TryGetValue(".WebScout.Tenant", out var cSlug) && !string.IsNullOrWhiteSpace(cSlug))
            {
                var slug = cSlug.Trim().ToLowerInvariant();
                AplicarPresetPorSlug(slug);
                if (_isResolved) return;
            }

            // Subdominio
            var host = http.Request.Host.Host;
            if (host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase))
            {
                var slug = host.Split('.')[0].ToLowerInvariant();
                AplicarPresetPorSlug(slug);
                if (_isResolved) return;
            }

            // Fallback en desarrollo para peticiones web en localhost sin subdominio
            SetTenant(OrgLestonnac, "lestonnac", "Grupo Scout Lestonnac 330", 330);
        }
    }

    private void AplicarPresetPorSlug(string slug)
    {
        if (slug.Equals("sanvicente", StringComparison.OrdinalIgnoreCase))
        {
            SetTenant(OrgSanVicente, "sanvicente", "Grupo Scout San Vicente 412", 412);
        }
        else if (slug.Equals("lestonnac", StringComparison.OrdinalIgnoreCase))
        {
            SetTenant(OrgLestonnac, "lestonnac", "Grupo Scout Lestonnac 330", 330);
        }
    }

    public void SetTenant(Guid tenantId, string slug, string groupName, int groupNumber)
    {
        _currentTenantId = tenantId;
        _tenantSlug = slug;
        _groupName = groupName;
        _groupNumber = groupNumber;
        _isResolved = true;
    }

    public void Clear()
    {
        _currentTenantId = Guid.Empty;
        _tenantSlug = string.Empty;
        _groupName = string.Empty;
        _groupNumber = 0;
        _isResolved = false;
    }
}
