using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebScout.Web.Features.Audit.Services;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Orders.Services;
using WebScout.Web.Infrastructure.Tenancy;

namespace WebScout.Web.Features.Orders.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/pedidos/aprovisionamiento/csv", async (
            IServicioPedidosMaterial servicioPedidos,
            ITenantContext tenant,
            IServicioAuditoria auditoria,
            HttpContext httpContext) =>
        {
            try
            {
                var orgId = tenant.CurrentTenantId;
                var (bytes, nombreArchivo) = await servicioPedidos.GenerarCsvAprovisionamientoAsync(orgId);

                await auditoria.RegistrarAsync(
                    accion: "EXPORTAR_CSV_APROVISIONAMIENTO",
                    tipoRecurso: "PedidoMaterial",
                    recursoId: orgId.ToString(),
                    detalles: $"Descarga de informe CSV para aprovisionamiento de material scout ({nombreArchivo})");

                return Results.File(
                    bytes,
                    contentType: "text/csv; charset=utf-8",
                    fileDownloadName: nombreArchivo);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .RequireAuthorization(policy => policy.RequireRole(
            RolesScout.Scouter,
            RolesScout.Coordinacion,
            RolesScout.Administracion,
            RolesScout.SuperAdmin));

        return endpoints;
    }
}
