using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Admissions.Domain;
using WebScout.Web.Features.Admissions.Templates;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Infrastructure.Outbox.Services;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;

namespace WebScout.Web.Features.Admissions.Services;

/// <summary>
/// Implementación del servicio de admisiones y contacto con persistencia transaccional y notificaciones Outbox (BL03).
/// </summary>
public class ServicioAdmisiones : IServicioAdmisiones
{
    private readonly WebScoutDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IServicioOutbox _servicioOutbox;
    private readonly ILogger<ServicioAdmisiones> _logger;

    public ServicioAdmisiones(
        WebScoutDbContext dbContext,
        ITenantContext tenantContext,
        IServicioOutbox servicioOutbox,
        ILogger<ServicioAdmisiones> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _servicioOutbox = servicioOutbox;
        _logger = logger;
    }

    public async Task<SolicitudAdmision> RegistrarSolicitudAdmisionAsync(
        RegistrarSolicitudAdmisionDto dto,
        CancellationToken cancellationToken = default)
    {
        ValidarTenantActivo();

        if (!dto.ConsentimientoPrivacidad)
        {
            throw new InvalidOperationException("Debe aceptar expresamente la política de privacidad para tramitar la solicitud de admisión (Art. 7 RGPD).");
        }

        if (string.IsNullOrWhiteSpace(dto.NombreEducando) || string.IsNullOrWhiteSpace(dto.ApellidosEducando))
        {
            throw new ArgumentException("El nombre y apellidos del educando son obligatorios.");
        }

        if (string.IsNullOrWhiteSpace(dto.EmailTutor) || string.IsNullOrWhiteSpace(dto.TelefonoTutor))
        {
            throw new ArgumentException("El correo y teléfono del tutor son obligatorios.");
        }

        var ramaSugerida = SolicitudAdmision.CalcularRamaPedagogica(dto.FechaNacimiento);

        var solicitud = new SolicitudAdmision
        {
            Id = Guid.NewGuid(),
            OrganizacionId = _tenantContext.CurrentTenantId,
            NombreEducando = dto.NombreEducando.Trim(),
            ApellidosEducando = dto.ApellidosEducando.Trim(),
            FechaNacimiento = dto.FechaNacimiento,
            RamaSugerida = ramaSugerida,
            NombreTutor = dto.NombreTutor.Trim(),
            RelacionTutor = string.IsNullOrWhiteSpace(dto.RelacionTutor) ? "Madre/Padre/Tutor legal" : dto.RelacionTutor.Trim(),
            EmailTutor = dto.EmailTutor.Trim(),
            TelefonoTutor = dto.TelefonoTutor.Trim(),
            Observaciones = dto.Observaciones?.Trim(),
            ConsentimientoPrivacidad = dto.ConsentimientoPrivacidad,
            ConsentimientoComunicaciones = dto.ConsentimientoComunicaciones,
            Estado = EstadoSolicitudAdmision.Pendiente,
            FechaSolicitudUtc = DateTime.UtcNow
        };

        await _dbContext.SolicitudesAdmision.AddAsync(solicitud, cancellationToken);

        // Despacho transaccional Outbox (PA11 / RF14)
        var grupo = await _dbContext.GruposScout.FindAsync([_tenantContext.CurrentTenantId], cancellationToken);
        var nombreGrupo = grupo?.Nombre ?? "Grupo Scout";
        var emailContactoGrupo = grupo?.EmailContacto;

        // 1. Acuse de recibo a la familia
        var acuse = PlantillasCorreoAdmision.AcuseSolicitudAdmision(
            dto.NombreTutor,
            solicitud.NombreCompletoEducando,
            ramaSugerida.ObtenerNombre(),
            nombreGrupo);

        await _servicioOutbox.EncolarMensajeAsync(
            organizacionId: _tenantContext.CurrentTenantId,
            tipoEvento: "SolicitudAdmisionPresentada",
            destinatarioEmail: dto.EmailTutor,
            destinatarioNombre: dto.NombreTutor,
            asunto: acuse.Asunto,
            cuerpoHtml: acuse.CuerpoHtml,
            cuerpoTextoPlano: acuse.CuerpoTextoPlano,
            claveDeduplicacion: $"solicitud-admision-acuse-{solicitud.Id}",
            replyToEmail: emailContactoGrupo,
            remitenteNombre: nombreGrupo,
            guardarInmediatamente: false,
            cancellationToken: cancellationToken);

        // 2. Notificación interna para secretaría/Kraal
        if (!string.IsNullOrWhiteSpace(emailContactoGrupo))
        {
            var avisoKraal = PlantillasCorreoAdmision.AvisoNuevaSolicitudKraal(
                solicitud.NombreCompletoEducando,
                ramaSugerida.ObtenerNombre(),
                dto.NombreTutor,
                dto.TelefonoTutor,
                nombreGrupo);

            await _servicioOutbox.EncolarMensajeAsync(
                organizacionId: _tenantContext.CurrentTenantId,
                tipoEvento: "AvisoSecretariaNuevaAdmision",
                destinatarioEmail: emailContactoGrupo,
                destinatarioNombre: $"Secretaría {nombreGrupo}",
                asunto: avisoKraal.Asunto,
                cuerpoHtml: avisoKraal.CuerpoHtml,
                cuerpoTextoPlano: avisoKraal.CuerpoTextoPlano,
                claveDeduplicacion: $"solicitud-admision-kraal-{solicitud.Id}",
                replyToEmail: dto.EmailTutor,
                remitenteNombre: "WebScout Admisiones",
                guardarInmediatamente: false,
                cancellationToken: cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Solicitud de admisión {SolicitudId} registrada para '{Educando}' en rama {Rama} ({OrganizacionId})",
            solicitud.Id, solicitud.NombreCompletoEducando, ramaSugerida, _tenantContext.CurrentTenantId);

        return solicitud;
    }

    public async Task<MensajeContacto> RegistrarMensajeContactoAsync(
        RegistrarMensajeContactoDto dto,
        CancellationToken cancellationToken = default)
    {
        ValidarTenantActivo();

        if (!dto.ConsentimientoPrivacidad)
        {
            throw new InvalidOperationException("Debe aceptar la política de privacidad para enviar el mensaje de contacto.");
        }

        if (string.IsNullOrWhiteSpace(dto.NombreRemitente) || string.IsNullOrWhiteSpace(dto.EmailRemitente))
        {
            throw new ArgumentException("El nombre y el correo electrónico son obligatorios.");
        }

        if (string.IsNullOrWhiteSpace(dto.Asunto) || string.IsNullOrWhiteSpace(dto.Mensaje))
        {
            throw new ArgumentException("El asunto y el contenido del mensaje son obligatorios.");
        }

        var mensaje = new MensajeContacto
        {
            Id = Guid.NewGuid(),
            OrganizacionId = _tenantContext.CurrentTenantId,
            NombreRemitente = dto.NombreRemitente.Trim(),
            EmailRemitente = dto.EmailRemitente.Trim(),
            TelefonoRemitente = dto.TelefonoRemitente?.Trim(),
            Asunto = dto.Asunto.Trim(),
            Mensaje = dto.Mensaje.Trim(),
            ConsentimientoPrivacidad = dto.ConsentimientoPrivacidad,
            Leido = false,
            Respondido = false,
            FechaEnvioUtc = DateTime.UtcNow
        };

        await _dbContext.MensajesContacto.AddAsync(mensaje, cancellationToken);

        // Despacho transaccional Outbox
        var grupo = await _dbContext.GruposScout.FindAsync([_tenantContext.CurrentTenantId], cancellationToken);
        var nombreGrupo = grupo?.Nombre ?? "Grupo Scout";
        var emailContactoGrupo = grupo?.EmailContacto;

        var acuse = PlantillasCorreoAdmision.AcuseMensajeContacto(
            dto.NombreRemitente,
            dto.Asunto,
            nombreGrupo);

        await _servicioOutbox.EncolarMensajeAsync(
            organizacionId: _tenantContext.CurrentTenantId,
            tipoEvento: "MensajeContactoPresentado",
            destinatarioEmail: dto.EmailRemitente,
            destinatarioNombre: dto.NombreRemitente,
            asunto: acuse.Asunto,
            cuerpoHtml: acuse.CuerpoHtml,
            cuerpoTextoPlano: acuse.CuerpoTextoPlano,
            claveDeduplicacion: $"mensaje-contacto-{mensaje.Id}",
            replyToEmail: emailContactoGrupo,
            remitenteNombre: nombreGrupo,
            guardarInmediatamente: false,
            cancellationToken: cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Mensaje de contacto {MensajeId} registrado de {Remitente}", mensaje.Id, dto.EmailRemitente);

        return mensaje;
    }

    public async Task<IReadOnlyList<SolicitudAdmision>> ObtenerSolicitudesAsync(
        EstadoSolicitudAdmision? filtroEstado = null,
        RamaScout? filtroRama = null,
        CancellationToken cancellationToken = default)
    {
        ValidarTenantActivo();

        var query = _dbContext.SolicitudesAdmision.AsNoTracking();

        if (filtroEstado.HasValue)
        {
            query = query.Where(s => s.Estado == filtroEstado.Value);
        }

        if (filtroRama.HasValue)
        {
            query = query.Where(s => s.RamaSugerida == filtroRama.Value);
        }

        return await query
            .OrderBy(s => s.FechaSolicitudUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MensajeContacto>> ObtenerMensajesContactoAsync(
        bool? soloNoLeidos = null,
        CancellationToken cancellationToken = default)
    {
        ValidarTenantActivo();

        var query = _dbContext.MensajesContacto.AsNoTracking();

        if (soloNoLeidos.HasValue && soloNoLeidos.Value)
        {
            query = query.Where(m => !m.Leido);
        }

        return await query
            .OrderByDescending(m => m.FechaEnvioUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task CambiarEstadoSolicitudAsync(
        Guid solicitudId,
        EstadoSolicitudAdmision nuevoEstado,
        string? notasKraal,
        Guid scouterId,
        CancellationToken cancellationToken = default)
    {
        ValidarTenantActivo();

        var solicitud = await _dbContext.SolicitudesAdmision
            .FirstOrDefaultAsync(s => s.Id == solicitudId, cancellationToken);

        if (solicitud == null)
        {
            throw new InvalidOperationException("La solicitud de admisión no existe o no pertenece a este grupo scout.");
        }

        solicitud.Estado = nuevoEstado;
        solicitud.NotasKraal = notasKraal;
        solicitud.AtendidoPorUsuarioId = scouterId;
        solicitud.FechaResolucionUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Solicitud {SolicitudId} actualizada a estado {Estado} por Scouter {ScouterId}",
            solicitudId, nuevoEstado, scouterId);
    }

    public async Task MarcarMensajeLeidoAsync(
        Guid mensajeId,
        bool leido,
        CancellationToken cancellationToken = default)
    {
        ValidarTenantActivo();

        var mensaje = await _dbContext.MensajesContacto
            .FirstOrDefaultAsync(m => m.Id == mensajeId, cancellationToken);

        if (mensaje == null)
        {
            throw new InvalidOperationException("El mensaje no existe o no pertenece a este grupo scout.");
        }

        mensaje.Leido = leido;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void ValidarTenantActivo()
    {
        if (!_tenantContext.IsResolved || _tenantContext.CurrentTenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Operación no permitida sin una Organización activa.");
        }
    }
}
