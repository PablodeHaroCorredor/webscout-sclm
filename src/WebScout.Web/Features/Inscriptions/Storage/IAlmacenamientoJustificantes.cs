namespace WebScout.Web.Features.Inscriptions.Storage;

/// <summary>
/// Contrato para el almacenamiento seguro y segregado de justificantes de pago bancario.
/// </summary>
public interface IAlmacenamientoJustificantes
{
    /// <summary>
    /// Almacena un archivo de justificante en una ubicación segura y devuelve su ruta relativa identificativa.
    /// </summary>
    Task<string> GuardarJustificanteAsync(
        Guid organizacionId,
        Guid inscripcionId,
        string nombreArchivoOriginal,
        Stream streamContenido);

    /// <summary>
    /// Recupera el flujo y metadatos de un justificante almacenado previamente.
    /// </summary>
    Task<(Stream Stream, string ContentType, string NombreDescarga)?> ObtenerJustificanteAsync(string rutaRelativa);

    /// <summary>
    /// Elimina el archivo físico de un justificante.
    /// </summary>
    Task EliminarJustificanteAsync(string rutaRelativa);

    /// <summary>
    /// Comprueba que el almacén está accesible y admite escritura. Lo usa el panel de diagnóstico,
    /// que no debe mostrar el almacén como operativo sin haberlo comprobado.
    /// </summary>
    Task<(bool Disponible, string Detalle)> ComprobarDisponibilidadAsync() =>
        Task.FromResult((false, "Este almacén no implementa comprobación de disponibilidad."));
}
