using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebScout.Web.Infrastructure.Outbox.Services;
using WebScout.Web.Infrastructure.Outbox.Workers;

namespace WebScout.Web.Infrastructure.Outbox;

/// <summary>
/// Métodos de extensión para el registro del motor Outbox y transporte SMTP en el contenedor de dependencias.
/// </summary>
public static class OutboxServiceExtensions
{
    public static IServiceCollection AddOutboxInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ConfiguracionSmtp>(configuration.GetSection(ConfiguracionSmtp.Seccion));

        services.AddScoped<IServicioEmail, ServicioEmailSmtp>();
        services.AddScoped<IServicioOutbox, ServicioOutbox>();
        services.AddHostedService<WorkerDespachadorOutbox>();

        return services;
    }
}
