using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace WebScout.Web.Features.Inscriptions.Storage;

public class AlmacenamientoJustificantesLocal : IAlmacenamientoJustificantes
{
    private readonly string _directorioBase;
    private readonly ILogger<AlmacenamientoJustificantesLocal> _logger;

    private const long MaxTamanoBytes = 10 * 1024 * 1024; // Límite máximo defensivo de 10 MB

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

        // 1. Verificación defensiva de cabecera binaria (Magic Bytes) para evitar spoofing de extensión
        var bufferCabecera = new byte[16];
        var bytesLeidos = await streamContenido.ReadAsync(bufferCabecera.AsMemory(0, bufferCabecera.Length));
        if (!EsFirmaBinariaValida(bufferCabecera, bytesLeidos, extension))
        {
            throw new InvalidOperationException($"El contenido binario del archivo no coincide con un formato legítimo {extension.ToUpperInvariant()}.");
        }

        var dirTenant = Path.Combine(_directorioBase, organizacionId.ToString());
        if (!Directory.Exists(dirTenant))
        {
            Directory.CreateDirectory(dirTenant);
        }

        var nombreFisico = $"{inscripcionId}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var rutaCompleta = Path.Combine(dirTenant, nombreFisico);

        try
        {
            await using (var fs = new FileStream(rutaCompleta, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                // Escribir la cabecera leída previamente
                await fs.WriteAsync(bufferCabecera.AsMemory(0, bytesLeidos));

                // Copiar el resto del stream vigilando el límite de 10 MB
                var bufferCopia = new byte[81920];
                long totalBytes = bytesLeidos;
                int leidos;

                while ((leidos = await streamContenido.ReadAsync(bufferCopia.AsMemory(0, bufferCopia.Length))) > 0)
                {
                    totalBytes += leidos;
                    if (totalBytes > MaxTamanoBytes)
                    {
                        throw new InvalidOperationException("El archivo excede el tamaño máximo permitido de 10 MB.");
                    }
                    await fs.WriteAsync(bufferCopia.AsMemory(0, leidos));
                }
            }
        }
        catch
        {
            // Limpieza en caso de error o exceso de cuota
            if (File.Exists(rutaCompleta))
            {
                try { File.Delete(rutaCompleta); } catch { }
            }
            throw;
        }

        _logger.LogInformation("Justificante guardado en {RutaCompleta} para inscripción {InscripcionId}", rutaCompleta, inscripcionId);

        // Devolver la ruta relativa a la base de justificantes (ej. "{organizacionId}/{nombreFisico}")
        return Path.Combine(organizacionId.ToString(), nombreFisico).Replace('\\', '/');
    }

    private static bool EsFirmaBinariaValida(byte[] cabecera, int leidos, string extension)
    {
        if (leidos < 3) return false;

        switch (extension.ToLowerInvariant())
        {
            case ".pdf":
                // %PDF-
                return leidos >= 5 && cabecera[0] == 0x25 && cabecera[1] == 0x50 && cabecera[2] == 0x44 && cabecera[3] == 0x46 && cabecera[4] == 0x2D;

            case ".jpg":
            case ".jpeg":
                // FF D8 FF
                return cabecera[0] == 0xFF && cabecera[1] == 0xD8 && cabecera[2] == 0xFF;

            case ".png":
                // 89 50 4E 47 0D 0A 1A 0A
                return leidos >= 8 && cabecera[0] == 0x89 && cabecera[1] == 0x50 && cabecera[2] == 0x4E && cabecera[3] == 0x47 &&
                                      cabecera[4] == 0x0D && cabecera[5] == 0x0A && cabecera[6] == 0x1A && cabecera[7] == 0x0A;

            case ".webp":
                // RIFF .... WEBP
                return leidos >= 12 && cabecera[0] == (byte)'R' && cabecera[1] == (byte)'I' && cabecera[2] == (byte)'F' && cabecera[3] == (byte)'F' &&
                                       cabecera[8] == (byte)'W' && cabecera[9] == (byte)'E' && cabecera[10] == (byte)'B' && cabecera[11] == (byte)'P';

            default:
                return false;
        }
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

    public async Task<(bool Disponible, string Detalle)> ComprobarDisponibilidadAsync()
    {
        try
        {
            Directory.CreateDirectory(_directorioBase);
            var sonda = Path.Combine(_directorioBase, $".sonda-{Guid.NewGuid():N}.tmp");
            await File.WriteAllTextAsync(sonda, "ok");
            File.Delete(sonda);
            return (true, "Almacén local en disco del servidor: accesible y con permiso de escritura.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "El almacén local de justificantes no admite escritura en {Directorio}", _directorioBase);
            return (false, "El almacén local de justificantes no admite escritura. Revisar permisos del servidor.");
        }
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
