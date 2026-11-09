using WebScout.Web.Common.Domain;

namespace WebScout.Web.Features.Admissions.Domain;

/// <summary>
/// Mensaje o consulta pública enviada al buzón del grupo scout (RF10, RF11 / BL03).
/// </summary>
public class MensajeContacto : IEntidadOrganizacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identificador del grupo scout destinatario (aislamiento multitenant estricto).
    /// </summary>
    public Guid OrganizacionId { get; set; }

    public string NombreRemitente { get; set; } = string.Empty;
    public string EmailRemitente { get; set; } = string.Empty;
    public string? TelefonoRemitente { get; set; }
    public string Asunto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Consentimiento explícito de privacidad (obligatorio, PA13).
    /// </summary>
    public bool ConsentimientoPrivacidad { get; set; }

    public bool Leido { get; set; } = false;
    public bool Respondido { get; set; } = false;
    public string? NotasKraal { get; set; }

    public DateTime FechaEnvioUtc { get; set; } = DateTime.UtcNow;
}
