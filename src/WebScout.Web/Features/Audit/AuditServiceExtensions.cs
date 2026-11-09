using Microsoft.Extensions.DependencyInjection;
using WebScout.Web.Features.Audit.Services;

namespace WebScout.Web.Features.Audit;

public static class AuditServiceExtensions
{
    public static IServiceCollection AddScoutAudit(this IServiceCollection services)
    {
        services.AddScoped<IServicioAuditoria, ServicioAuditoria>();
        return services;
    }
}
