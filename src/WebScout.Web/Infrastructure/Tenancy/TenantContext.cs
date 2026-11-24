using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Identity.Domain;

namespace WebScout.Web.Infrastructure.Tenancy;

/// <summary>
/// Implementación con ámbito (scoped) de ITenantContext para la solicitud HTTP actual o circuito Blazor.
/// En peticiones HTTP lo fija <see cref="TenantResolutionMiddleware"/>. En el circuito interactivo
/// (ámbito nuevo, sin middleware) se resuelve de forma perezosa con el mismo extractor de slug
/// (<see cref="ExtractorSlugTenant"/>) y el mismo <see cref="IResolutorGrupoScout"/> sobre <c>GruposScout</c>.
/// No hay grupos escritos en código ni grupo de respaldo.
/// </summary>
public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly AuthenticationStateProvider? _authStateProvider;
    private readonly IResolutorGrupoScout? _resolutor;
    private readonly string _baseDomain = ExtractorSlugTenant.DominioBasePorDefecto;

    private Guid _currentTenantId = Guid.Empty;
    private string _tenantSlug = string.Empty;
    private string _groupName = string.Empty;
    private int _groupNumber;
    private PaletteConfig? _palette;
    private bool _isResolved;
    private bool _isSuperAdmin;
    private bool _resolucionIntentada;

    public TenantContext()
    {
    }

    public TenantContext(
        IHttpContextAccessor? httpContextAccessor = null,
        AuthenticationStateProvider? authStateProvider = null,
        IResolutorGrupoScout? resolutor = null,
        IConfiguration? configuration = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _authStateProvider = authStateProvider;
        _resolutor = resolutor;
        _baseDomain = configuration?.GetValue<string>("Tenancy:BaseDomain") ?? ExtractorSlugTenant.DominioBasePorDefecto;
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
        if (_isResolved || _resolucionIntentada) return;

        var http = _httpContextAccessor?.HttpContext;
        if (http != null)
        {
            // Solo se marca como intentada si había petición que inspeccionar: sin HttpContext se
            // reintenta en el siguiente acceso. Con petición, un slug desconocido no vuelve a consultarse.
            _resolucionIntentada = true;
            var slug = ExtractorSlugTenant.Extraer(http.Request, _baseDomain);

            if (string.IsNullOrWhiteSpace(slug) || PortalFederativo.EsSlugFederativo(slug))
            {
                // Dominio raíz, www, admin o sclm: portal federativo
                SetTenant(Guid.Empty, PortalFederativo.Slug, PortalFederativo.Nombre, 0);
            }
            else
            {
                // Grupo scout: se resuelve contra GruposScout (solo activos). Un slug desconocido
                // o inactivo deja el contexto sin resolver (OrganizacionId vacío): nunca se cae a otro grupo.
                var grupo = _resolutor?.Resolver(slug);
                if (grupo != null)
                {
                    SetTenant(grupo.Id, grupo.Slug, grupo.Nombre, grupo.NumeroGrupo, grupo.Paleta);
                }
            }
        }

        VerificarSuperAdmin();
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
        _resolucionIntentada = false;
    }
}
