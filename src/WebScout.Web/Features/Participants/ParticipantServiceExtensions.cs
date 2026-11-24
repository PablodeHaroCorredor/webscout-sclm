using WebScout.Web.Features.Participants.Services;

namespace WebScout.Web.Features.Participants;

public static class ParticipantServiceExtensions
{
    public static IServiceCollection AddParticipantFeatures(this IServiceCollection services)
    {
        services.AddScoped<ServicioEducandos>();
        services.AddScoped<IServicioEducandos>(sp => sp.GetRequiredService<ServicioEducandos>());
        services.AddScoped<IParticipantService>(sp => sp.GetRequiredService<ServicioEducandos>());
        services.AddScoped<IServicioGestionSeccion, ServicioGestionSeccion>();
        return services;
    }
}
