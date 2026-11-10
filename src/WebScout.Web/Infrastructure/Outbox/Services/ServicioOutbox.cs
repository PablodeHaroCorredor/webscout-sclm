using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebScout.Web.Infrastructure.Outbox.Domain;
using WebScout.Web.Infrastructure.Persistence;

namespace WebScout.Web.Infrastructure.Outbox.Services;

/// <summary>
/// Implementación del servicio Outbox transaccional para garantizar la persistencia atómica
/// de notificaciones y su despacho asíncrono con reintentos y deduplicación (ADR07 / PA11).
/// </summary>
public class ServicioOutbox : IServicioOutbox
{
    private readonly WebScoutDbContext _dbContext;
    private readonly IServicioEmail _servicioEmail;
    private readonly ILogger<ServicioOutbox> _logger;

    public ServicioOutbox(
        WebScoutDbContext dbContext,
        IServicioEmail servicioEmail,
        ILogger<ServicioOutbox> logger)
    {
        _dbContext = dbContext;
        _servicioEmail = servicioEmail;
        _logger = logger;
    }

    public async Task<MensajeOutbox> EncolarMensajeAsync(
        Guid organizacionId,
        string tipoEvento,
        string destinatarioEmail,
        string destinatarioNombre,
        string asunto,
        string cuerpoHtml,
        string cuerpoTextoPlano,
        string? claveDeduplicacion = null,
        string? replyToEmail = null,
        string? remitenteNombre = null,
        bool guardarInmediatamente = true,
        CancellationToken cancellationToken = default)
    {
        if (organizacionId == Guid.Empty)
        {
            throw new ArgumentException("El OrganizacionId es obligatorio para garantizar el aislamiento multitenant del mensaje.", nameof(organizacionId));
        }

        if (string.IsNullOrWhiteSpace(destinatarioEmail))
        {
            throw new ArgumentException("El correo del destinatario es obligatorio.", nameof(destinatarioEmail));
        }

        // Sanitización contra CRLF en asunto y nombres
        var asuntoSanitizado = ServicioEmailSmtp.SanitizarCabecera(asunto);
        var nombreDestinatarioSanitizado = ServicioEmailSmtp.SanitizarCabecera(destinatarioNombre);

        // Clave de deduplicación determinista (o autogenerada si no se proporciona)
        var clave = !string.IsNullOrWhiteSpace(claveDeduplicacion)
            ? claveDeduplicacion.Trim()
            : $"{tipoEvento}_{Guid.NewGuid():N}";

        // Verificar deduplicación existente dentro de la misma organización (PA11)
        var existente = await _dbContext.MensajesOutbox
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                m => m.OrganizacionId == organizacionId && m.ClaveDeduplicacion == clave,
                cancellationToken);

        if (existente != null)
        {
            _logger.LogInformation(
                "Mensaje Outbox omitido por deduplicación existente. Clave: {ClaveDeduplicacion}, Estado: {Estado}",
                clave, existente.Estado);
            return existente;
        }

        var nuevoMensaje = new MensajeOutbox
        {
            Id = Guid.NewGuid(),
            OrganizacionId = organizacionId,
            TipoEvento = tipoEvento,
            ClaveDeduplicacion = clave,
            DestinatarioEmail = destinatarioEmail.Trim(),
            DestinatarioNombre = nombreDestinatarioSanitizado,
            Asunto = asuntoSanitizado,
            ReplyToEmail = replyToEmail?.Trim(),
            RemitenteNombre = remitenteNombre?.Trim(),
            CuerpoHtml = cuerpoHtml,
            CuerpoTextoPlano = cuerpoTextoPlano,
            Estado = EstadoMensajeOutbox.Pendiente,
            Reintentos = 0,
            MaxReintentos = 5,
            ProximoReintentoUtc = DateTime.UtcNow,
            FechaCreacionUtc = DateTime.UtcNow
        };

        await _dbContext.MensajesOutbox.AddAsync(nuevoMensaje, cancellationToken);

        if (guardarInmediatamente)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Mensaje Outbox {MensajeId} persistido para {Destinatario} con clave {ClaveDeduplicacion}",
                nuevoMensaje.Id, nuevoMensaje.DestinatarioEmail, clave);
        }

        return nuevoMensaje;
    }

    public async Task<int> ProcesarLotePendienteAsync(int tamanoLote = 20, CancellationToken cancellationToken = default)
    {
        var ahora = DateTime.UtcNow;

        // Recuperar mensajes listos para envío en orden cronológico, ignorando filtros de tenant (trabajo federativo)
        var mensajes = await _dbContext.MensajesOutbox
            .IgnoreQueryFilters()
            .Where(m => (m.Estado == EstadoMensajeOutbox.Pendiente || m.Estado == EstadoMensajeOutbox.EnProceso)
                        && (m.ProximoReintentoUtc == null || m.ProximoReintentoUtc <= ahora)
                        && m.Reintentos < m.MaxReintentos)
            .OrderBy(m => m.FechaCreacionUtc)
            .Take(tamanoLote)
            .ToListAsync(cancellationToken);

        if (mensajes.Count == 0)
        {
            return 0;
        }

        _logger.LogInformation("Procesando lote Outbox de {Cantidad} mensajes...", mensajes.Count);
        var enviadosExitosos = 0;

        foreach (var mensaje in mensajes)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            mensaje.Estado = EstadoMensajeOutbox.EnProceso;
            await _dbContext.SaveChangesAsync(cancellationToken);

            try
            {
                await _servicioEmail.EnviarEmailAsync(
                    destinatarioEmail: mensaje.DestinatarioEmail,
                    destinatarioNombre: mensaje.DestinatarioNombre,
                    asunto: mensaje.Asunto,
                    cuerpoHtml: mensaje.CuerpoHtml,
                    cuerpoTextoPlano: mensaje.CuerpoTextoPlano,
                    remitenteNombre: mensaje.RemitenteNombre,
                    replyToEmail: mensaje.ReplyToEmail,
                    messageId: mensaje.ClaveDeduplicacion,
                    cancellationToken: cancellationToken);

                mensaje.Estado = EstadoMensajeOutbox.Enviado;
                mensaje.FechaProcesadoUtc = DateTime.UtcNow;
                mensaje.ErrorUltimoReintento = null;
                enviadosExitosos++;

                _logger.LogInformation(
                    "Mensaje Outbox {MensajeId} enviado satisfactoriamente a {Destinatario}",
                    mensaje.Id, mensaje.DestinatarioEmail);
            }
            catch (Exception ex)
            {
                mensaje.Reintentos++;
                mensaje.ErrorUltimoReintento = ex.Message;

                if (mensaje.Reintentos >= mensaje.MaxReintentos)
                {
                    mensaje.Estado = EstadoMensajeOutbox.Fallido;
                    _logger.LogWarning(
                        "Mensaje Outbox {MensajeId} alcanzó el límite de {MaxReintentos} intentos. Marcado como Fallido.",
                        mensaje.Id, mensaje.MaxReintentos);
                }
                else
                {
                    mensaje.Estado = EstadoMensajeOutbox.Pendiente;
                    // Backoff exponencial con jitter: 2^reintentos * 10 segundos + jitter
                    var segundosBase = Math.Pow(2, mensaje.Reintentos) * 10;
                    var jitter = Random.Shared.Next(1, 10);
                    mensaje.ProximoReintentoUtc = DateTime.UtcNow.AddSeconds(segundosBase + jitter);

                    _logger.LogWarning(
                        "Fallo al despachar mensaje Outbox {MensajeId} (intento {Reintentos}/{MaxReintentos}): {Error}. Próximo reintento: {ProximoReintentoUtc}",
                        mensaje.Id, mensaje.Reintentos, mensaje.MaxReintentos, ex.Message, mensaje.ProximoReintentoUtc);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return enviadosExitosos;
    }

    public async Task<IReadOnlyList<MensajeOutbox>> ObtenerMensajesAsync(
        EstadoMensajeOutbox? filtroEstado = null,
        string? busqueda = null,
        int limite = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MensajesOutbox.AsQueryable();

        if (filtroEstado.HasValue)
        {
            query = query.Where(m => m.Estado == filtroEstado.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.Trim().ToLower();
            query = query.Where(m => m.DestinatarioEmail.ToLower().Contains(b)
                                  || m.Asunto.ToLower().Contains(b)
                                  || m.TipoEvento.ToLower().Contains(b));
        }

        return await query
            .OrderByDescending(m => m.FechaCreacionUtc)
            .Take(limite)
            .ToListAsync(cancellationToken);
    }

    public async Task<ResumenOutboxDto> ObtenerResumenEstadisticasAsync(CancellationToken cancellationToken = default)
    {
        var total = await _dbContext.MensajesOutbox.CountAsync(cancellationToken);
        var pendientes = await _dbContext.MensajesOutbox.CountAsync(m => m.Estado == EstadoMensajeOutbox.Pendiente && m.Reintentos == 0, cancellationToken);
        var enviados = await _dbContext.MensajesOutbox.CountAsync(m => m.Estado == EstadoMensajeOutbox.Enviado, cancellationToken);
        var enReintento = await _dbContext.MensajesOutbox.CountAsync(m => m.Estado == EstadoMensajeOutbox.Pendiente && m.Reintentos > 0, cancellationToken);
        var fallidos = await _dbContext.MensajesOutbox.CountAsync(m => m.Estado == EstadoMensajeOutbox.Fallido, cancellationToken);

        return new ResumenOutboxDto(total, pendientes, enviados, enReintento, fallidos);
    }

    public async Task ReintentarMensajeAsync(Guid mensajeId, CancellationToken cancellationToken = default)
    {
        var mensaje = await _dbContext.MensajesOutbox.FirstOrDefaultAsync(m => m.Id == mensajeId, cancellationToken);
        if (mensaje == null)
        {
            throw new KeyNotFoundException($"No se encontró el mensaje Outbox con ID {mensajeId}.");
        }

        mensaje.Estado = EstadoMensajeOutbox.Pendiente;
        mensaje.ProximoReintentoUtc = DateTime.UtcNow;
        mensaje.Reintentos = 0; // Se reinicia para permitir un nuevo ciclo completo

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Mensaje Outbox {MensajeId} restablecido para reintento manual inmediato.", mensajeId);
    }
}
