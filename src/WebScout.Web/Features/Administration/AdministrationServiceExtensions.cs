using Microsoft.Extensions.DependencyInjection;
using WebScout.Web.Features.Administration.Services;

namespace WebScout.Web.Features.Administration;

public static class AdministrationServiceExtensions
{
    public static IServiceCollection AddAdministrationFeatures(this IServiceCollection services)
    {
        services.AddScoped<IServicioConfiguracionGrupo, ServicioConfiguracionGrupo>();
        return services;
    }
}
