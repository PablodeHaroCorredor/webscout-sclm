using WebScout.Web.Features.Inscriptions.Services;
using WebScout.Web.Features.Inscriptions.Storage;

namespace WebScout.Web.Features.Inscriptions;

public static class InscriptionServiceExtensions
{
    public static IServiceCollection AddInscriptionFeatures(this IServiceCollection services)
    {
        services.AddSingleton<IAlmacenamientoJustificantes, AlmacenamientoJustificantesLocal>();
        services.AddScoped<IServicioInscripciones, ServicioInscripciones>();
        return services;
    }
}
