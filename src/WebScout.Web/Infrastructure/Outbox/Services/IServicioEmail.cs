namespace WebScout.Web.Infrastructure.Outbox.Services;

/// <summary>
/// Contrato para el despacho de correos electrónicos a través del transporte SMTP.
/// </summary>
public interface IServicioEmail
{
    /// <summary>
    /// Envía un correo electrónico sanitizando cabeceras y aplicando Message-ID determinista.
    /// </summary>
    Task EnviarEmailAsync(
        string destinatarioEmail,
        string destinatarioNombre,
        string asunto,
        string cuerpoHtml,
        string cuerpoTextoPlano,
        string? remitenteNombre = null,
        string? replyToEmail = null,
        string? messageId = null,
        CancellationToken cancellationToken = default);
}
