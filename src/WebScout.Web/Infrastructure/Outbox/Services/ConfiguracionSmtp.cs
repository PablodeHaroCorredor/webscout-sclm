namespace WebScout.Web.Infrastructure.Outbox.Services;

/// <summary>
/// Opciones de configuración para el servidor de correo saliente SMTP (Mailpit en desarrollo / SMTP en producción).
/// </summary>
public class ConfiguracionSmtp
{
    public const string Seccion = "Smtp";

    /// <summary>
    /// Dirección del servidor SMTP (por defecto 'localhost' para Mailpit).
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Puerto de escucha SMTP (1025 para Mailpit por defecto, 587/465 para producción).
    /// </summary>
    public int Puerto { get; set; } = 1025;

    /// <summary>
    /// Indica si se debe requerir cifrado TLS/SSL en el transporte.
    /// </summary>
    public bool HabilitarSsl { get; set; } = false;

    /// <summary>
    /// Usuario de autenticación SMTP opcional.
    /// </summary>
    public string? Usuario { get; set; }

    /// <summary>
    /// Contraseña de autenticación SMTP opcional.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Dirección de correo por defecto del remitente federativo.
    /// </summary>
    public string RemitentePorDefectoEmail { get; set; } = "noreply@scoutsclm.org";

    /// <summary>
    /// Nombre visible por defecto del remitente.
    /// </summary>
    public string RemitentePorDefectoNombre { get; set; } = "WebScout SCLM";
}
