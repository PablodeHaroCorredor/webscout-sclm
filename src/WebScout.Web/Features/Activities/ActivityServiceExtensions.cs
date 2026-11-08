using WebScout.Web.Features.Activities.Services;

namespace WebScout.Web.Features.Activities;

public static class ActivityServiceExtensions
{
    public static IServiceCollection AddActivityFeatures(this IServiceCollection services)
    {
        services.AddScoped<IServicioActividades, ServicioActividades>();
        return services;
    }
}
