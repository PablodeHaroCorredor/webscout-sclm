using System.Net;

namespace WebScout.Web.Infrastructure.Outbox.Templates;

/// <summary>
/// Generador de plantillas de correo responsive y seguras con identidad scout y cumplimiento estricto
/// de minimización de datos (RNF09 / RGPD: sin datos médicos, diagnósticos ni PII bancaria en el cuerpo).
/// </summary>
public static class PlantillasCorreo
{
    public record ContenidoCorreo(string Asunto, string CuerpoHtml, string CuerpoTextoPlano);

    /// <summary>
    /// Genera la notificación de requerimiento de subsanación documental en una inscripción.
    /// </summary>
    public static ContenidoCorreo SubsanacionRequerida(
        string nombreEducando,
        string tituloActividad,
        string? motivoKraal,
        string urlAcceso,
        string nombreGrupo)
    {
        var educandoSeguro = WebUtility.HtmlEncode(nombreEducando);
        var actividadSegura = WebUtility.HtmlEncode(tituloActividad);
        var grupoSeguro = WebUtility.HtmlEncode(nombreGrupo);
        var motivoSeguro = !string.IsNullOrWhiteSpace(motivoKraal)
            ? WebUtility.HtmlEncode(motivoKraal)
            : "Por favor, revise la documentación y justificantes aportados en el portal.";

        var asunto = $"[{grupoSeguro}] Acción requerida: Subsanación en la inscripción de {nombreEducando}";

        var cuerpoHtml = RenderizarLayout(
            grupoSeguro,
            "Requerimiento de subsanación documental",
            $@"
            <p style=""font-size: 16px; color: #1e293b; line-height: 1.6;"">
                Hola, familia:
            </p>
            <p style=""font-size: 15px; color: #334155; line-height: 1.6;"">
                El equipo de Scouters (Kraal) de <strong>{grupoSeguro}</strong> ha revisado la solicitud de inscripción de <strong>{educandoSeguro}</strong> para la actividad <strong>{actividadSegura}</strong> y requiere la subsanación de ciertos documentos para poder confirmar su plaza.
            </p>
            <div style=""background-color: #fef2f2; border-left: 4px solid #ef4444; padding: 16px; border-radius: 4px; margin: 20px 0;"">
                <p style=""margin: 0; font-size: 14px; font-weight: 600; color: #991b1b;"">Indicaciones del Kraal:</p>
                <p style=""margin: 6px 0 0 0; font-size: 14px; color: #7f1d1d;"">{motivoSeguro}</p>
            </div>
            <p style=""font-size: 14px; color: #64748b; line-height: 1.5;"">
                Por motivos de privacidad y seguridad sanitaria (RGPD), no adjuntamos justificantes bancarios ni informes clínicos por correo electrónico. Acceda a su área familiar en el portal para cargar la documentación actualizada.
            </p>
            <div style=""text-align: center; margin: 30px 0;"">
                <a href=""{urlAcceso}"" style=""background-color: #0f766e; color: #ffffff; text-decoration: none; padding: 12px 28px; border-radius: 8px; font-weight: 600; font-size: 15px; display: inline-block;"">
                    Subsanar Documentación en el Portal
                </a>
            </div>",
            urlAcceso);

        var cuerpoTextoPlano = $@"Hola, familia:

El equipo de Scouters de {nombreGrupo} ha revisado la inscripción de {nombreEducando} para '{tituloActividad}'.
Se requiere subsanar la siguiente documentación:

{motivoKraal ?? "Por favor, revise la documentación aportada en el portal."}

Por seguridad y protección de datos, no enviamos detalles bancarios ni sanitarios por correo.
Acceda a su área familiar para completar la subsanación:
{urlAcceso}

Siempre Listos,
Kraal de {nombreGrupo}";

        return new ContenidoCorreo(asunto, cuerpoHtml, cuerpoTextoPlano);
    }

    /// <summary>
    /// Genera la notificación de confirmación definitiva de plaza en una actividad.
    /// </summary>
    public static ContenidoCorreo ConfirmacionInscripcion(
        string nombreEducando,
        string tituloActividad,
        DateTime? fechaInicio,
        string lugar,
        string urlAcceso,
        string nombreGrupo)
    {
        var educandoSeguro = WebUtility.HtmlEncode(nombreEducando);
        var actividadSegura = WebUtility.HtmlEncode(tituloActividad);
        var grupoSeguro = WebUtility.HtmlEncode(nombreGrupo);
        var lugarSeguro = WebUtility.HtmlEncode(lugar);
        var fechaSegura = fechaInicio.HasValue ? fechaInicio.Value.ToString("dd/MM/yyyy HH:mm") : "Por determinar";

        var asunto = $"[{grupoSeguro}] ¡Plaza confirmada! Inscripción de {nombreEducando} en {tituloActividad}";

        var cuerpoHtml = RenderizarLayout(
            grupoSeguro,
            "¡Inscripción confirmada!",
            $@"
            <p style=""font-size: 16px; color: #1e293b; line-height: 1.6;"">
                Hola, familia:
            </p>
            <p style=""font-size: 15px; color: #334155; line-height: 1.6;"">
                Nos complace informaros de que la inscripción de <strong>{educandoSeguro}</strong> para la actividad <strong>{actividadSegura}</strong> ha sido verificada y <strong>confirmada formalmente</strong>.
            </p>
            <div style=""background-color: #f0fdf4; border-left: 4px solid #16a34a; padding: 16px; border-radius: 4px; margin: 20px 0;"">
                <p style=""margin: 0; font-size: 14px; color: #166534;""><strong>Fecha de inicio:</strong> {fechaSegura}</p>
                <p style=""margin: 6px 0 0 0; font-size: 14px; color: #166534;""><strong>Lugar:</strong> {lugarSeguro}</p>
            </div>
            <p style=""font-size: 14px; color: #64748b; line-height: 1.5;"">
                Podéis consultar los detalles de la actividad, horarios de salida, material necesario y autorizaciones en vuestro panel familiar:
            </p>
            <div style=""text-align: center; margin: 30px 0;"">
                <a href=""{urlAcceso}"" style=""background-color: #16a34a; color: #ffffff; text-decoration: none; padding: 12px 28px; border-radius: 8px; font-weight: 600; font-size: 15px; display: inline-block;"">
                    Ver Detalles de la Actividad
                </a>
            </div>",
            urlAcceso);

        var cuerpoTextoPlano = $@"Hola, familia:

La inscripción de {nombreEducando} para la actividad '{tituloActividad}' en {nombreGrupo} ha sido confirmada con éxito.

Fecha de inicio: {fechaSegura}
Lugar: {lugar}

Accede al portal para ver el material necesario y horarios:
{urlAcceso}

Siempre Listos,
Kraal de {nombreGrupo}";

        return new ContenidoCorreo(asunto, cuerpoHtml, cuerpoTextoPlano);
    }

    /// <summary>
    /// Genera la notificación de acuse de recibo de solicitud de inscripción presentada.
    /// </summary>
    public static ContenidoCorreo InscripcionPresentada(
        string nombreEducando,
        string tituloActividad,
        string urlAcceso,
        string nombreGrupo)
    {
        var educandoSeguro = WebUtility.HtmlEncode(nombreEducando);
        var actividadSegura = WebUtility.HtmlEncode(tituloActividad);
        var grupoSeguro = WebUtility.HtmlEncode(nombreGrupo);

        var asunto = $"[{grupoSeguro}] Solicitud recibida: Inscripción de {nombreEducando} en {tituloActividad}";

        var cuerpoHtml = RenderizarLayout(
            grupoSeguro,
            "Solicitud de inscripción recibida",
            $@"
            <p style=""font-size: 16px; color: #1e293b; line-height: 1.6;"">
                Hola, familia:
            </p>
            <p style=""font-size: 15px; color: #334155; line-height: 1.6;"">
                Hemos recibido correctamente vuestra solicitud de inscripción para <strong>{educandoSeguro}</strong> en la actividad <strong>{actividadSegura}</strong>.
            </p>
            <p style=""font-size: 14px; color: #475569; line-height: 1.6;"">
                El equipo de Scouters revisará la documentación aportada. Si la actividad requiere pago y no habéis adjuntado aún el justificante bancario, podéis aportarlo desde vuestro panel familiar en cualquier momento antes de la fecha límite.
            </p>
            <div style=""text-align: center; margin: 30px 0;"">
                <a href=""{urlAcceso}"" style=""background-color: #0f766e; color: #ffffff; text-decoration: none; padding: 12px 28px; border-radius: 8px; font-weight: 600; font-size: 15px; display: inline-block;"">
                    Consultar Estado de la Solicitud
                </a>
            </div>",
            urlAcceso);

        var cuerpoTextoPlano = $@"Hola, familia:

Hemos recibido la solicitud de inscripción de {nombreEducando} para la actividad '{tituloActividad}' en {nombreGrupo}.
El Kraal revisará los datos aportados. Puedes consultar el estado en cualquier momento en:
{urlAcceso}

Siempre Listos,
Kraal de {nombreGrupo}";

        return new ContenidoCorreo(asunto, cuerpoHtml, cuerpoTextoPlano);
    }

    private static string RenderizarLayout(string nombreGrupo, string encabezado, string contenidoPrincipal, string urlAcceso)
    {
        return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{encabezado}</title>
</head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 24px; color: #1e293b;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -2px rgba(0, 0, 0, 0.05); border: 1px solid #e2e8f0;"">
        <!-- Cabecera Institucional con Pañoleta Scout -->
        <div style=""background: linear-gradient(135deg, #0f766e 0%, #0d9488 100%); padding: 24px 32px; color: #ffffff;"">
            <span style=""font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 1.5px; opacity: 0.9;"">Scouts de Castilla-La Mancha</span>
            <h1 style=""margin: 6px 0 0 0; font-size: 22px; font-weight: 700; color: #ffffff;"">{nombreGrupo}</h1>
        </div>

        <!-- Contenedor del Cuerpo -->
        <div style=""padding: 32px;"">
            <h2 style=""margin-top: 0; font-size: 18px; color: #0f172a; font-weight: 700; border-bottom: 2px solid #f1f5f9; padding-bottom: 12px;"">{encabezado}</h2>
            {contenidoPrincipal}
        </div>

        <!-- Pie Legal RGPD Federativo -->
        <div style=""background-color: #f8fafc; border-top: 1px solid #e2e8f0; padding: 20px 32px; font-size: 12px; color: #64748b; line-height: 1.5;"">
            <p style=""margin: 0 0 8px 0;"">
                Este es un mensaje transaccional automatizado enviado por <strong>WebScout SCLM</strong> en representación de {nombreGrupo}.
            </p>
            <p style=""margin: 0; font-size: 11px; color: #94a3b8;"">
                Conforme al Reglamento General de Protección de Datos (RGPD UE 2016/679), le informamos de que sus datos se tratan con la única finalidad de la gestión pedagógica y de seguridad en actividades scout. Para ejercer sus derechos de acceso, rectificación o supresión, contacte con su Kraal en el portal o con la federación.
            </p>
        </div>
    </div>
</body>
</html>";
    }
}
