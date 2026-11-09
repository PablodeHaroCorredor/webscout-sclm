using System.Net;
using WebScout.Web.Infrastructure.Outbox.Templates;

namespace WebScout.Web.Features.Admissions.Templates;

/// <summary>
/// Plantillas de correo electrónico transaccionales para solicitudes de admisión y buzón de contacto (RF14 / RNF09).
/// </summary>
public static class PlantillasCorreoAdmision
{
    public record ContenidoCorreo(string Asunto, string CuerpoHtml, string CuerpoTextoPlano);

    /// <summary>
    /// Notificación de acuse de recibo de solicitud de ingreso enviada a la familia.
    /// </summary>
    public static ContenidoCorreo AcuseSolicitudAdmision(
        string nombreTutor,
        string nombreEducando,
        string nombreRama,
        string nombreGrupo)
    {
        var tutorSeguro = WebUtility.HtmlEncode(nombreTutor);
        var educandoSeguro = WebUtility.HtmlEncode(nombreEducando);
        var ramaSegura = WebUtility.HtmlEncode(nombreRama);
        var grupoSeguro = WebUtility.HtmlEncode(nombreGrupo);

        var asunto = $"[{grupoSeguro}] Solicitud de admisión recibida: {nombreEducando}";

        var cuerpoHtml = $@"<!DOCTYPE html>
<html lang=""es"">
<head><meta charset=""UTF-8""><title>Solicitud de Admisión</title></head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 24px; color: #1e293b;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; border: 1px solid #e2e8f0;"">
        <div style=""background: #0f766e; padding: 24px 32px; color: #ffffff;"">
            <span style=""font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 1.5px; opacity: 0.9;"">Scouts de Castilla-La Mancha</span>
            <h1 style=""margin: 6px 0 0 0; font-size: 22px; font-weight: 700; color: #ffffff;"">{grupoSeguro}</h1>
        </div>
        <div style=""padding: 32px;"">
            <h2 style=""margin-top: 0; font-size: 18px; color: #0f172a; font-weight: 700; border-bottom: 2px solid #f1f5f9; padding-bottom: 12px;"">Solicitud de Admisión Recibida</h2>
            <p style=""font-size: 15px; color: #334155; line-height: 1.6;"">
                Hola, {tutorSeguro}:
            </p>
            <p style=""font-size: 15px; color: #334155; line-height: 1.6;"">
                Hemos recibido correctamente su solicitud de admisión para <strong>{educandoSeguro}</strong> en la sección pedagógica de <strong>{ramaSegura}</strong> de {grupoSeguro}.
            </p>
            <div style=""background-color: #f0fdf4; border-left: 4px solid #16a34a; padding: 16px; border-radius: 4px; margin: 20px 0;"">
                <p style=""margin: 0; font-size: 14px; color: #166534;""><strong>Estado del trámite:</strong> Registrada en lista de admisión de secretaría.</p>
                <p style=""margin: 6px 0 0 0; font-size: 13px; color: #166534;"">El equipo de secretaría del grupo revisará las plazas vacantes y se pondrá en contacto con usted por teléfono o correo electrónico.</p>
            </div>
            <p style=""font-size: 14px; color: #64748b; line-height: 1.5;"">
                Gracias por su interés en la educación scout y por confiar en nuestro proyecto asociativo.
            </p>
        </div>
        <div style=""background-color: #f8fafc; border-top: 1px solid #e2e8f0; padding: 20px 32px; font-size: 12px; color: #64748b;"">
            <p style=""margin: 0;"">Mensaje transaccional automático de {grupoSeguro} · Scouts de Castilla-La Mancha.</p>
        </div>
    </div>
</body>
</html>";

        var cuerpoTextoPlano = $@"Hola, {nombreTutor}:

Hemos recibido correctamente su solicitud de admisión para {nombreEducando} en {nombreRama} de {nombreGrupo}.
La solicitud ha quedado registrada en la secretaría del grupo, que revisará las vacantes y contactará con usted a la mayor brevedad.

Gracias por su interés en el escultismo.

Siempre Listos,
Secretaría de {nombreGrupo}";

        return new ContenidoCorreo(asunto, cuerpoHtml, cuerpoTextoPlano);
    }

    /// <summary>
    /// Notificación interna para el Kraal y secretaría informando de una nueva solicitud de plaza.
    /// </summary>
    public static ContenidoCorreo AvisoNuevaSolicitudKraal(
        string nombreEducando,
        string nombreRama,
        string nombreTutor,
        string telefonoTutor,
        string nombreGrupo)
    {
        var educandoSeguro = WebUtility.HtmlEncode(nombreEducando);
        var ramaSegura = WebUtility.HtmlEncode(nombreRama);
        var tutorSeguro = WebUtility.HtmlEncode(nombreTutor);
        var telefonoSeguro = WebUtility.HtmlEncode(telefonoTutor);
        var grupoSeguro = WebUtility.HtmlEncode(nombreGrupo);

        var asunto = $"[Aviso Secretaría] Nueva solicitud de admisión: {nombreEducando} ({nombreRama})";

        var cuerpoHtml = $@"<!DOCTYPE html>
<html lang=""es"">
<head><meta charset=""UTF-8""><title>Nueva Solicitud</title></head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 24px; color: #1e293b;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; border: 1px solid #e2e8f0;"">
        <div style=""background: #0f172a; padding: 20px 32px; color: #ffffff;"">
            <h2 style=""margin: 0; font-size: 18px; color: #ffffff;"">Buzón de Admisiones · {grupoSeguro}</h2>
        </div>
        <div style=""padding: 32px;"">
            <p style=""font-size: 15px; color: #334155;"">Se ha registrado una nueva solicitud de plaza en la web pública:</p>
            <ul style=""font-size: 14px; color: #1e293b; line-height: 1.8;"">
                <li><strong>Educando:</strong> {educandoSeguro}</li>
                <li><strong>Sección pedagógica:</strong> {ramaSegura}</li>
                <li><strong>Persona de contacto:</strong> {tutorSeguro}</li>
                <li><strong>Teléfono:</strong> {telefonoSeguro}</li>
            </ul>
            <p style=""font-size: 14px; color: #64748b;"">Accede al panel de gestión del Kraal para revisar la lista de espera y gestionar la admisión.</p>
        </div>
    </div>
</body>
</html>";

        var cuerpoTextoPlano = $@"Aviso de Secretaría - {nombreGrupo}

Nueva solicitud de plaza:
- Educando: {nombreEducando}
- Rama: {nombreRama}
- Tutor/a: {nombreTutor}
- Teléfono: {telefonoTutor}

Accede al panel de gestión del Kraal para tramitarla.";

        return new ContenidoCorreo(asunto, cuerpoHtml, cuerpoTextoPlano);
    }

    /// <summary>
    /// Notificación de acuse de recibo de mensaje de contacto general.
    /// </summary>
    public static ContenidoCorreo AcuseMensajeContacto(string nombreRemitente, string asuntoConsulta, string nombreGrupo)
    {
        var remitenteSeguro = WebUtility.HtmlEncode(nombreRemitente);
        var asuntoSeguro = WebUtility.HtmlEncode(asuntoConsulta);
        var grupoSeguro = WebUtility.HtmlEncode(nombreGrupo);

        var asunto = $"[{grupoSeguro}] Hemos recibido su mensaje: {asuntoConsulta}";

        var cuerpoHtml = $@"<!DOCTYPE html>
<html lang=""es"">
<head><meta charset=""UTF-8""><title>Mensaje Recibido</title></head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 24px; color: #1e293b;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; border: 1px solid #e2e8f0;"">
        <div style=""background: #0f766e; padding: 20px 32px; color: #ffffff;"">
            <h1 style=""margin: 0; font-size: 20px; color: #ffffff;"">{grupoSeguro}</h1>
        </div>
        <div style=""padding: 32px;"">
            <p style=""font-size: 15px; color: #334155; line-height: 1.6;"">
                Hola, {remitenteSeguro}:
            </p>
            <p style=""font-size: 15px; color: #334155; line-height: 1.6;"">
                Hemos recibido su consulta con asunto <strong>'{asuntoSeguro}'</strong>. El equipo responsable del grupo le responderá con la mayor brevedad posible.
            </p>
            <p style=""font-size: 14px; color: #64748b;"">Gracias por contactar con nosotros.</p>
        </div>
    </div>
</body>
</html>";

        var cuerpoTextoPlano = $@"Hola, {nombreRemitente}:

Hemos recibido su consulta ('{asuntoConsulta}').
El equipo de {nombreGrupo} le responderá a la mayor brevedad posible.

Siempre Listos,
{nombreGrupo}";

        return new ContenidoCorreo(asunto, cuerpoHtml, cuerpoTextoPlano);
    }
}
