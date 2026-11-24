using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebScout.Web.Infrastructure.Persistence;

namespace WebScout.Web.Infrastructure.Tenancy;

public static class TenantServiceExtensions
{
    public static IServiceCollection AddScoutTenancy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        // Registrar el contexto de tenant con ciclo de vida Scoped (por petición HTTP o circuito Blazor)
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        // Resolución de grupos por slug contra GruposScout, con caché corta e invalidación al guardar
        services.AddSingleton<IResolutorGrupoScout, ResolutorGrupoScout>();
        services.AddSingleton<InvalidacionCacheGruposInterceptor>();

        // Registrar DbContext con PostgreSQL / Npgsql
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5433;Database=webscout_dev;Username=scout_admin;Password=scout_dev_password_change_me";

        services.AddDbContext<WebScoutDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<InvalidacionCacheGruposInterceptor>());
        });

        return services;
    }

    public static IApplicationBuilder UseScoutTenancy(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantResolutionMiddleware>();
        return app;
    }

    public static IApplicationBuilder UseTenantSessionGuard(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantSessionGuardMiddleware>();
        return app;
    }
}
