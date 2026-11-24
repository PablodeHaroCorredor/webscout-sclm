using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Admissions.Domain;
using WebScout.Web.Features.Admissions.Templates;
using WebScout.Web.Features.Audit.Domain;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Identity.Services;
using WebScout.Web.Features.Participants.Domain;
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
    private readonly IServicioAlcanceSeccion _alcance;

    public ServicioAdmisiones(
        WebScoutDbContext dbContext,
        ITenantContext tenantContext,
        IServicioOutbox servicioOutbox,
        ILogger<ServicioAdmisiones> logger,
        IServicioAlcanceSeccion alcance)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _servicioOutbox = servicioOutbox;
        _logger = logger;
        _alcance = alcance;
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

        // Solicitudes de admisión: datos de menores. Las ve quien puede incorporarlos a la sección sugerida
        // (alcance global, o el Coordinador de esa sección); un Scouter sin coordinación no las ve.
        var alcance = await _alcance.ObtenerAlcanceAsync();
        if (!alcance.EsGlobal && alcance.Funcion != FuncionSeccion.Coordinador)
        {
            return [];
        }

        var query = _dbContext.SolicitudesAdmision.AsNoTracking();

        if (filtroEstado.HasValue)
        {
            query = query.Where(s => s.Estado == filtroEstado.Value);
        }

        var ramaConsulta = alcance.RamaDeConsulta(filtroRama);
        if (ramaConsulta.HasValue)
        {
            query = query.Where(s => s.RamaSugerida == ramaConsulta.Value);
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

        var alcance = await _alcance.ObtenerAlcanceAsync();
        if (solicitud == null || !alcance.PuedeGestionar(solicitud.RamaSugerida))
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

    public async Task<Educando> ConvertirSolicitudEnEducandoAsync(
        Guid solicitudId,
        RamaScout ramaAsignada,
        string? notasKraal,
        Guid scouterId,
        CancellationToken cancellationToken = default)
    {
        ValidarTenantActivo();

        var solicitud = await _dbContext.SolicitudesAdmision
            .FirstOrDefaultAsync(s => s.Id == solicitudId, cancellationToken);

        // Un Coordinador de sección solo incorpora solicitudes de su sección y a su sección;
        // el alcance global puede asignar cualquier rama.
        var alcance = await _alcance.ObtenerAlcanceAsync();
        if (solicitud == null || !alcance.PuedeGestionar(solicitud.RamaSugerida) || !alcance.PuedeGestionar(ramaAsignada))
        {
            throw new KeyNotFoundException($"No se encontró la solicitud de admisión con ID {solicitudId}.");
        }

        if (solicitud.Estado is EstadoSolicitudAdmision.Rechazada or EstadoSolicitudAdmision.Archivada)
        {
            throw new InvalidOperationException("No se puede incorporar al censo una solicitud rechazada o archivada.");
        }

        // Evita duplicar al educando si la solicitud ya se incorporó (doble clic, segunda pestaña o solicitud repetida).
        // La comparación de nombres se hace en memoria (sin distinguir mayúsculas ni tildes) para no depender de la intercalación de la base de datos.
        var mismosNacimiento = await _dbContext.Educandos
            .Where(e => e.FechaNacimiento == solicitud.FechaNacimiento)
            .Select(e => new { e.Nombre, e.Apellidos })
            .ToListAsync(cancellationToken);
        var comparador = System.Globalization.CultureInfo.GetCultureInfo("es-ES").CompareInfo;
        const System.Globalization.CompareOptions opciones =
            System.Globalization.CompareOptions.IgnoreCase | System.Globalization.CompareOptions.IgnoreNonSpace;
        var yaEnCenso = mismosNacimiento.Any(e =>
            comparador.Compare(e.Nombre.Trim(), solicitud.NombreEducando.Trim(), opciones) == 0 &&
            comparador.Compare(e.Apellidos.Trim(), solicitud.ApellidosEducando.Trim(), opciones) == 0);

        if (yaEnCenso)
        {
            throw new InvalidOperationException(
                $"{solicitud.NombreCompletoEducando} ya figura en el censo del grupo con la misma fecha de nacimiento. No se ha creado un duplicado.");
        }

        var tutorEmailNormalizado = solicitud.EmailTutor.Trim().ToUpperInvariant();
        var tutor = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.OrganizacionId == _tenantContext.CurrentTenantId
                                   && u.NormalizedEmail == tutorEmailNormalizado, cancellationToken);

        if (tutor == null)
        {
            tutor = new UsuarioScout
            {
                Id = Guid.NewGuid(),
                OrganizacionId = _tenantContext.CurrentTenantId,
                UserName = solicitud.EmailTutor.Trim(),
                NormalizedUserName = tutorEmailNormalizado,
                Email = solicitud.EmailTutor.Trim(),
                NormalizedEmail = tutorEmailNormalizado,
                NombreCompleto = solicitud.NombreTutor.Trim(),
                PhoneNumber = solicitud.TelefonoTutor.Trim(),
                SecurityStamp = Guid.NewGuid().ToString("D"),
                EmailConfirmed = true
            };

            await _dbContext.Users.AddAsync(tutor, cancellationToken);

            var rolFamilia = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.NormalizedName == "FAMILIA", cancellationToken);

            if (rolFamilia != null)
            {
                await _dbContext.UserRoles.AddAsync(new Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>
                {
                    UserId = tutor.Id,
                    RoleId = rolFamilia.Id
                }, cancellationToken);
            }

            _logger.LogInformation("Creada nueva cuenta familiar '{Email}' para el tutor de {Educando}",
                solicitud.EmailTutor, solicitud.NombreCompletoEducando);
        }

        var educando = new Educando
        {
            Id = Guid.NewGuid(),
            OrganizacionId = _tenantContext.CurrentTenantId,
            TutorLegalId = tutor.Id,
            Nombre = solicitud.NombreEducando.Trim(),
            Apellidos = solicitud.ApellidosEducando.Trim(),
            FechaNacimiento = solicitud.FechaNacimiento,
            RamaActual = ramaAsignada,
            Activo = true,
            FechaCreacionUtc = DateTime.UtcNow
        };

        await _dbContext.Educandos.AddAsync(educando, cancellationToken);

        var ficha = new FichaMedica
        {
            Id = Guid.NewGuid(),
            OrganizacionId = _tenantContext.CurrentTenantId,
            EducandoId = educando.Id,
            NumeroSeguridadSocial = "",
            TelefonoEmergenciaPrincipal = solicitud.TelefonoTutor.Trim(),
            NombreContactoEmergenciaPrincipal = solicitud.NombreTutor.Trim(),
            UltimaActualizacionUtc = DateTime.UtcNow
        };

        await _dbContext.FichasMedicas.AddAsync(ficha, cancellationToken);

        // Despacho transaccional Outbox de bienvenida (PA11 / RF14)
        var grupo = await _dbContext.GruposScout.FindAsync([_tenantContext.CurrentTenantId], cancellationToken);
        var nombreGrupo = grupo?.Nombre ?? "Grupo Scout";

        var cuerpoHtml = $"""
            <div style="font-family: sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #E2E8F0; border-radius: 12px;">
                <h2 style="color: #0F172A;">¡Bienvenida al {nombreGrupo}!</h2>
                <p>Estimada familia de <strong>{solicitud.NombreCompletoEducando}</strong>:</p>
                <p>Nos complace comunicaros que la solicitud de admisión ha sido <strong>admitida</strong> y el educando ha sido incorporado formalmente al censo en la sección de <strong>{ramaAsignada.ObtenerNombre()}</strong>.</p>
                <p>Se ha activado vuestra cuenta familiar en el <strong>Portal de Familias</strong> vinculada al correo <code>{solicitud.EmailTutor}</code>.</p>
                <p>Ya podéis acceder al portal para revisar y completar la ficha médica anual, consultar las próximas salidas de fin de semana y gestionar el material scout:</p>
                <p style="text-align: center; margin: 30px 0;">
                    <a href="/login" style="background-color: #10B981; color: white; padding: 12px 24px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;">Acceder al Portal de Familias</a>
                </p>
                <p style="font-size: 0.85em; color: #64748B;">Siempre Listos,<br/>Kraal de Scouters y Secretaría · {nombreGrupo}</p>
            </div>
            """;

        var cuerpoTexto = $"¡Bienvenida al {nombreGrupo}! {solicitud.NombreCompletoEducando} ha sido admitido en {ramaAsignada.ObtenerNombre()}. Accede al Portal de Familias en /login con tu email {solicitud.EmailTutor}.";

        await _servicioOutbox.EncolarMensajeAsync(
            organizacionId: _tenantContext.CurrentTenantId,
            tipoEvento: "BienvenidaAltaEducandoCenso",
            destinatarioEmail: solicitud.EmailTutor,
            destinatarioNombre: solicitud.NombreTutor,
            asunto: $"¡Bienvenida al {nombreGrupo}! - Alta en Censo de {solicitud.NombreCompletoEducando}",
            cuerpoHtml: cuerpoHtml,
            cuerpoTextoPlano: cuerpoTexto,
            claveDeduplicacion: $"alta-censo-{solicitud.Id}",
            replyToEmail: grupo?.EmailContacto,
            remitenteNombre: nombreGrupo,
            guardarInmediatamente: false,
            cancellationToken: cancellationToken);

        solicitud.Estado = EstadoSolicitudAdmision.Admitida;
        solicitud.AtendidoPorUsuarioId = scouterId;
        solicitud.FechaResolucionUtc = DateTime.UtcNow;
        solicitud.NotasKraal = !string.IsNullOrWhiteSpace(notasKraal)
            ? notasKraal
            : $"Incorporado a {ramaAsignada.ObtenerNombre()} como educando activo por Secretaría.";

        var eventoAuditoria = new RegistroAuditoria
        {
            OrganizacionId = _tenantContext.CurrentTenantId,
            UsuarioId = scouterId,
            Accion = "AltaEducandoDesdeAdmision",
            TipoRecurso = nameof(Educando),
            RecursoId = educando.Id.ToString(),
            Detalles = $"Educando {educando.NombreCompleto} admitido en rama {ramaAsignada.ObtenerNombre()}. Tutor: {solicitud.EmailTutor}",
            FechaCreacionUtc = DateTime.UtcNow
        };

        await _dbContext.RegistrosAuditoria.AddAsync(eventoAuditoria, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Educando {EducandoId} ({Nombre}) creado con éxito desde la solicitud {SolicitudId} en la rama {Rama}",
            educando.Id, educando.NombreCompleto, solicitudId, ramaAsignada);

        return educando;
    }

    private void ValidarTenantActivo()
    {
        if (!_tenantContext.IsResolved || _tenantContext.CurrentTenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Operación no permitida sin una Organización activa.");
        }
    }
}
