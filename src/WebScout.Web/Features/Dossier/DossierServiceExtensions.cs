using WebScout.Web.Features.Dossier.Services;

namespace WebScout.Web.Features.Dossier;

public static class DossierServiceExtensions
{
    public static IServiceCollection AddDossierFeatures(this IServiceCollection services)
    {
        services.AddScoped<IServicioDossierAsistencia, ServicioDossierAsistencia>();
        return services;
    }
}
