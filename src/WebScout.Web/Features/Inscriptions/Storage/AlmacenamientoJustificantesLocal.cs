using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace WebScout.Web.Features.Inscriptions.Storage;

public class AlmacenamientoJustificantesLocal : IAlmacenamientoJustificantes
{
    private readonly string _directorioBase;
    private readonly ILogger<AlmacenamientoJustificantesLocal> _logger;

    private static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".webp"
    };

    public AlmacenamientoJustificantesLocal(
        IWebHostEnvironment env,
        ILogger<AlmacenamientoJustificantesLocal> logger)
    {
        _logger = logger;
        _directorioBase = Path.Combine(env.ContentRootPath, "App_Data", "justificantes");

        if (!Directory.Exists(_directorioBase))
        {
            Directory.CreateDirectory(_directorioBase);
        }
    }

    public async Task<string> GuardarJustificanteAsync(
        Guid organizacionId,
        Guid inscripcionId,
        string nombreArchivoOriginal,
        Stream streamContenido)
    {
        var extension = Path.GetExtension(nombreArchivoOriginal);
        if (string.IsNullOrWhiteSpace(extension) || !ExtensionesPermitidas.Contains(extension))
        {
            throw new InvalidOperationException($"El formato de archivo '{extension}' no está permitido. Solo se aceptan PDF, JPG, PNG o WEBP.");
        }

        var dirTenant = Path.Combine(_directorioBase, organizacionId.ToString());
        if (!Directory.Exists(dirTenant))
        {
            Directory.CreateDirectory(dirTenant);
        }

        var nombreFisico = $"{inscripcionId}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var rutaCompleta = Path.Combine(dirTenant, nombreFisico);

        using (var fs = new FileStream(rutaCompleta, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await streamContenido.CopyToAsync(fs);
        }

        _logger.LogInformation("Justificante guardado en {RutaCompleta} para inscripción {InscripcionId}", rutaCompleta, inscripcionId);

        // Devolver la ruta relativa a la base de justificantes (ej. "{organizacionId}/{nombreFisico}")
        return Path.Combine(organizacionId.ToString(), nombreFisico).Replace('\\', '/');
    }

    public Task<(Stream Stream, string ContentType, string NombreDescarga)?> ObtenerJustificanteAsync(string rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa))
        {
            return Task.FromResult<(Stream Stream, string ContentType, string NombreDescarga)?>(null);
        }

        var rutaSanitizada = rutaRelativa.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var rutaCompleta = Path.Combine(_directorioBase, rutaSanitizada);

        // Prevenir Directory Traversal
        var rutaNormalizada = Path.GetFullPath(rutaCompleta);
        if (!rutaNormalizada.StartsWith(_directorioBase, StringComparison.OrdinalIgnoreCase) || !File.Exists(rutaNormalizada))
        {
            return Task.FromResult<(Stream Stream, string ContentType, string NombreDescarga)?>(null);
        }

        var extension = Path.GetExtension(rutaNormalizada).ToLowerInvariant();
        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        var fs = new FileStream(rutaNormalizada, FileMode.Open, FileAccess.Read, FileShare.Read);
        var nombreDescarga = Path.GetFileName(rutaNormalizada);

        return Task.FromResult<(Stream Stream, string ContentType, string NombreDescarga)?>((fs, contentType, nombreDescarga));
    }

    public Task EliminarJustificanteAsync(string rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa))
        {
            return Task.CompletedTask;
        }

        var rutaSanitizada = rutaRelativa.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var rutaCompleta = Path.Combine(_directorioBase, rutaSanitizada);
        var rutaNormalizada = Path.GetFullPath(rutaCompleta);

        if (rutaNormalizada.StartsWith(_directorioBase, StringComparison.OrdinalIgnoreCase) && File.Exists(rutaNormalizada))
        {
            File.Delete(rutaNormalizada);
            _logger.LogInformation("Justificante eliminado: {RutaNormalizada}", rutaNormalizada);
        }

        return Task.CompletedTask;
    }
}
