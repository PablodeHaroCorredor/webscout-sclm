using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Infrastructure.Persistence;

namespace WebScout.Web.Features.Identity;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddScoutIdentity(this IServiceCollection services)
    {
        services.AddIdentity<ScoutUser, ScoutRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.User.RequireUniqueEmail = false; // El índice único es compuesto: (OrganizationId, NormalizedEmail)
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<WebScoutDbContext>()
        .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/acceso-denegado";
            options.Cookie.Name = ".WebScout.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
        });

        services.AddAuthentication();
        services.AddAuthorization();
        services.AddCascadingAuthenticationState();

        return services;
    }
}
