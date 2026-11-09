using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Inscriptions.Services;
using WebScout.Web.Features.Inscriptions.Storage;

namespace WebScout.Web.Features.Inscriptions.Endpoints;

/// <summary>
/// Endpoints para la descarga y visualización segura de justificantes de pago bancario adjuntos.
/// </summary>
public static class JustificanteEndpoints
{
    public static IEndpointRouteBuilder MapJustificanteEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/justificantes/{id:guid}", async (
            Guid id,
            IServicioInscripciones servicioInscripciones,
            IAlmacenamientoJustificantes almacenamiento,
            ClaimsPrincipal user) =>
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var inscripcion = await servicioInscripciones.ObtenerPorIdAsync(id);
            if (inscripcion == null || string.IsNullOrWhiteSpace(inscripcion.RutaArchivoJustificante))
            {
                return Results.NotFound("El comprobante de pago no existe o no se encuentra disponible.");
            }

            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var esTutor = Guid.TryParse(userIdStr, out var userId) && userId == inscripcion.TutorLegalId;
            var esKraal = user.IsInRole(RolesScout.Scouter) ||
                          user.IsInRole(RolesScout.Coordinacion) ||
                          user.IsInRole(RolesScout.Administracion);

            if (!esTutor && !esKraal)
            {
                return Results.Forbid();
            }

            var archivo = await almacenamiento.ObtenerJustificanteAsync(inscripcion.RutaArchivoJustificante);
            if (archivo == null)
            {
                return Results.NotFound("El archivo físico del justificante no ha podido ser localizado.");
            }

            return Results.File(
                archivo.Value.Stream,
                contentType: archivo.Value.ContentType,
                fileDownloadName: inscripcion.NombreArchivoJustificante ?? archivo.Value.NombreDescarga);
        }).RequireAuthorization();

        return endpoints;
    }
}
