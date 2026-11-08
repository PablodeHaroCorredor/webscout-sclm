using WebScout.Web.Features.Participants.Services;

namespace WebScout.Web.Features.Participants;

public static class ParticipantServiceExtensions
{
    public static IServiceCollection AddParticipantFeatures(this IServiceCollection services)
    {
        services.AddScoped<IParticipantService, ParticipantService>();
        return services;
    }
}
