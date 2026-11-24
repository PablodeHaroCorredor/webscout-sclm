using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebScout.Web.Features.Audit.Services;
using WebScout.Web.Features.Identity.Services;
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
            IServicioAuditoria servicioAuditoria,
            IServicioAlcanceSeccion servicioAlcance,
            ClaimsPrincipal user) =>
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            const string noDisponible = "El comprobante de pago no existe o no se encuentra disponible.";

            var inscripcion = await servicioInscripciones.ObtenerPorIdAsync(id);
            if (inscripcion == null || string.IsNullOrWhiteSpace(inscripcion.RutaArchivoJustificante))
            {
                return Results.NotFound(noDisponible);
            }

            // Acceso: el tutor titular, o quien tenga alcance sobre la sección del educando.
            // La denegación responde igual que un justificante inexistente.
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var esTutor = Guid.TryParse(userIdStr, out var userId) && userId == inscripcion.TutorLegalId;
            var alcance = esTutor ? null : await servicioAlcance.ObtenerAlcanceAsync();

            if (!esTutor && alcance?.PuedeVer(inscripcion.Educando.RamaActual) != true)
            {
                return Results.NotFound(noDisponible);
            }

            var archivo = await almacenamiento.ObtenerJustificanteAsync(inscripcion.RutaArchivoJustificante);
            if (archivo == null)
            {
                return Results.NotFound("El archivo físico del justificante no ha podido ser localizado.");
            }

            // Registrar auditoría de acceso al documento bancario
            await servicioAuditoria.RegistrarAsync(
                accion: "DescargaJustificante",
                tipoRecurso: "InscripcionActividad",
                recursoId: inscripcion.Id.ToString(),
                detalles: $"Archivo: {inscripcion.NombreArchivoJustificante}. Educando: {inscripcion.EducandoId}");

            return Results.File(
                archivo.Value.Stream,
                contentType: archivo.Value.ContentType,
                fileDownloadName: inscripcion.NombreArchivoJustificante ?? archivo.Value.NombreDescarga);
        }).RequireAuthorization();

        return endpoints;
    }
}
