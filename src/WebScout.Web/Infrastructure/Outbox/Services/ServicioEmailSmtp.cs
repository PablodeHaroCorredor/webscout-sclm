using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace WebScout.Web.Infrastructure.Outbox.Services;

/// <summary>
/// Implementación de envío de correo SMTP mediante MailKit y MimeKit, con protección estricta
/// contra inyección de cabeceras CRLF (CWE-93) y soporte RFC 5322 para Message-ID determinista.
/// </summary>
public class ServicioEmailSmtp : IServicioEmail
{
    private readonly ConfiguracionSmtp _configuracion;
    private readonly ILogger<ServicioEmailSmtp> _logger;

    public ServicioEmailSmtp(
        IOptions<ConfiguracionSmtp> configuracion,
        ILogger<ServicioEmailSmtp> logger)
    {
        _configuracion = configuracion.Value;
        _logger = logger;
    }

    public async Task EnviarEmailAsync(
        string destinatarioEmail,
        string destinatarioNombre,
        string asunto,
        string cuerpoHtml,
        string cuerpoTextoPlano,
        string? remitenteNombre = null,
        string? replyToEmail = null,
        string? messageId = null,
        CancellationToken cancellationToken = default)
    {
        var asuntoLimpio = SanitizarCabecera(asunto);
        var nombreDestinatarioLimpio = SanitizarCabecera(destinatarioNombre);
        var nombreRemitenteLimpio = SanitizarCabecera(remitenteNombre ?? _configuracion.RemitentePorDefectoNombre);

        var mensaje = new MimeMessage();

        mensaje.From.Add(new MailboxAddress(nombreRemitenteLimpio, _configuracion.RemitentePorDefectoEmail));
        mensaje.To.Add(new MailboxAddress(nombreDestinatarioLimpio, destinatarioEmail.Trim()));
        mensaje.Subject = asuntoLimpio;

        if (!string.IsNullOrWhiteSpace(replyToEmail))
        {
            mensaje.ReplyTo.Add(new MailboxAddress(nombreRemitenteLimpio, replyToEmail.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(messageId))
        {
            var idLimpio = SanitizarCabecera(messageId).Replace("@", "_");
            mensaje.MessageId = $"<{idLimpio}@scoutsclm.org>";
        }

        var builder = new BodyBuilder
        {
            HtmlBody = cuerpoHtml,
            TextBody = cuerpoTextoPlano
        };

        mensaje.Body = builder.ToMessageBody();

        using var client = new SmtpClient();

        try
        {
            var opcionesSsl = _configuracion.HabilitarSsl
                ? SecureSocketOptions.StartTlsWhenAvailable
                : SecureSocketOptions.None;

            await client.ConnectAsync(_configuracion.Host, _configuracion.Puerto, opcionesSsl, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_configuracion.Usuario) && !string.IsNullOrWhiteSpace(_configuracion.Password))
            {
                await client.AuthenticateAsync(_configuracion.Usuario, _configuracion.Password, cancellationToken);
            }

            await client.SendAsync(mensaje, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation(
                "Correo electrónico despachado exitosamente a {Destinatario} con asunto '{Asunto}'",
                destinatarioEmail, asuntoLimpio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo al enviar correo SMTP a {Destinatario}: {MensajeError}", destinatarioEmail, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Elimina retornos de carro y saltos de línea para neutralizar vulnerabilidades de Header Injection (CWE-93).
    /// </summary>
    public static string SanitizarCabecera(string? valor)
    {
        if (string.IsNullOrEmpty(valor))
            return string.Empty;

        return valor
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Trim();
    }
}
