using Microsoft.Extensions.DependencyInjection;
using WebScout.Web.Features.Admissions.Services;

namespace WebScout.Web.Features.Admissions;

public static class AdmissionServiceExtensions
{
    public static IServiceCollection AddAdmissionFeatures(this IServiceCollection services)
    {
        services.AddScoped<IServicioAdmisiones, ServicioAdmisiones>();
        return services;
    }
}
