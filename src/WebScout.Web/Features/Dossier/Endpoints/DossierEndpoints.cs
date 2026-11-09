using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Dossier.Services;

namespace WebScout.Web.Features.Dossier.Endpoints;

public static class DossierEndpoints
{
    public static IEndpointRouteBuilder MapDossierEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/actividades/{actividadId:guid}/asistencia/csv", async (
            Guid actividadId,
            IServicioDossierAsistencia servicioDossier) =>
        {
            try
            {
                var csv = await servicioDossier.GenerarCsvAsistenciaAsync(actividadId);
                var utf8Bom = Encoding.UTF8.GetPreamble();
                var csvBytes = Encoding.UTF8.GetBytes(csv);
                var fullBytes = new byte[utf8Bom.Length + csvBytes.Length];
                Buffer.BlockCopy(utf8Bom, 0, fullBytes, 0, utf8Bom.Length);
                Buffer.BlockCopy(csvBytes, 0, fullBytes, utf8Bom.Length, csvBytes.Length);

                return Results.File(
                    fullBytes,
                    contentType: "text/csv; charset=utf-8",
                    fileDownloadName: $"asistencia_actividad_{DateTime.UtcNow:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .RequireAuthorization(policy => policy.RequireRole(
            RolesScout.Scouter,
            RolesScout.Coordinacion,
            RolesScout.Administracion));

        return endpoints;
    }
}
