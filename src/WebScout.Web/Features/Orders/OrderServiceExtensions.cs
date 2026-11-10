using Microsoft.Extensions.DependencyInjection;
using WebScout.Web.Features.Orders.Services;

namespace WebScout.Web.Features.Orders;

public static class OrderServiceExtensions
{
    public static IServiceCollection AddOrderFeatures(this IServiceCollection services)
    {
        services.AddScoped<IServicioPedidosMaterial, ServicioPedidosMaterial>();
        return services;
    }
}
