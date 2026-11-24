using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebScout.Web.Common.Domain;
using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Identity.Services;
using WebScout.Web.Features.Inscriptions.Domain;
using WebScout.Web.Features.Inscriptions.Storage;
using WebScout.Web.Infrastructure.Outbox.Services;
using WebScout.Web.Infrastructure.Outbox.Templates;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;

namespace WebScout.Web.Features.Inscriptions.Services;

/// <summary>
/// Implementación del servicio de inscripciones a actividades scout y trazabilidad de justificantes de pago bancario.
/// Cumple aislamiento multitenant estricto y modelo de datos en castellano.
/// </summary>
public class ServicioInscripciones : IServicioInscripciones
{
    private readonly WebScoutDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IAlmacenamientoJustificantes _almacenamientoJustificantes;
    private readonly IServicioOutbox? _servicioOutbox;
    private readonly ILogger<ServicioInscripciones> _logger;
    private readonly IServicioAlcanceSeccion _alcance;

    public ServicioInscripciones(
        WebScoutDbContext dbContext,
        ITenantContext tenantContext,
        IAlmacenamientoJustificantes almacenamientoJustificantes,
        ILogger<ServicioInscripciones> logger,
        IServicioAlcanceSeccion alcance,
        IServicioOutbox? servicioOutbox = null)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _almacenamientoJustificantes = almacenamientoJustificantes;
        _logger = logger;
        _alcance = alcance;
        _servicioOutbox = servicioOutbox;
    }

    public async Task<InscripcionActividad> InscribirEducandoAsync(
        Guid actividadId,
        Guid educandoId,
        Guid tutorId,
        string? observaciones = null)
    {
        ValidarTenantActivo();

        var actividad = await _dbContext.Actividades
            .FirstOrDefaultAsync(a => a.Id == actividadId);

        if (actividad == null)
        {
            throw new InvalidOperationException("La actividad no existe o no pertenece a este grupo scout.");
        }

        if (actividad.Estado != EstadoActividad.Publicada)
        {
            throw new InvalidOperationException("No se pueden tramitar inscripciones en una actividad que no se encuentra en estado Publicada.");
        }

        if (actividad.FechaLimiteInscripcion.HasValue && actividad.FechaLimiteInscripcion.Value.Date < DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("El plazo de inscripción para esta actividad ha expirado.");
        }

        var educando = await _dbContext.Educandos
            .Include(e => e.FichaMedica)
            .FirstOrDefaultAsync(e => e.Id == educandoId);

        if (educando == null)
        {
            throw new InvalidOperationException("El educando no existe o no pertenece a este grupo scout.");
        }

        if (educando.TutorLegalId != tutorId)
        {
            throw new InvalidOperationException("No tiene autorización legal sobre el educando seleccionado.");
        }

        if (!educando.Activo)
        {
            throw new InvalidOperationException("El educando no se encuentra en estado activo en el censo del grupo.");
        }

        if (!actividad.AplicaARama(educando.RamaActual))
        {
            throw new InvalidOperationException($"La actividad no está dirigida a la rama {educando.RamaActual.ObtenerNombre()} a la que pertenece el educando.");
        }

        var inscripcionExistente = await _dbContext.Inscripciones
            .FirstOrDefaultAsync(i => i.ActividadId == actividadId && i.EducandoId == educandoId);

        if (inscripcionExistente != null && inscripcionExistente.Estado != EstadoInscripcion.Cancelada)
        {
            throw new InvalidOperationException("El educando ya cuenta con una inscripción activa o en trámite para esta actividad.");
        }

        var estadoInicial = actividad.Precio <= 0m
            ? EstadoInscripcion.Confirmada
            : EstadoInscripcion.PendienteJustificante;

        InscripcionActividad inscripcion;

        if (inscripcionExistente != null)
        {
            // Reactivación de inscripción previamente cancelada
            inscripcion = inscripcionExistente;
            inscripcion.Estado = estadoInicial;
            inscripcion.ImporteEuros = actividad.Precio;
            inscripcion.FechaInscripcionUtc = DateTime.UtcNow;
            inscripcion.AutorizacionFirmada = true;
            inscripcion.FechaAutorizacionUtc = DateTime.UtcNow;
            inscripcion.ObservacionesFamilia = observaciones;
            inscripcion.NotasKraal = null;
            inscripcion.NombreArchivoJustificante = null;
            inscripcion.RutaArchivoJustificante = null;
            inscripcion.FechaSubidaJustificanteUtc = null;
            inscripcion.RevisadoPorUsuarioId = null;
            inscripcion.FechaRevisionUtc = null;
        }
        else
        {
            inscripcion = new InscripcionActividad
            {
                OrganizacionId = _tenantContext.CurrentTenantId,
                ActividadId = actividadId,
                EducandoId = educandoId,
                TutorLegalId = tutorId,
                ImporteEuros = actividad.Precio,
                Estado = estadoInicial,
                AutorizacionFirmada = true,
                FechaAutorizacionUtc = DateTime.UtcNow,
                FechaInscripcionUtc = DateTime.UtcNow,
                ObservacionesFamilia = observaciones
            };

            _dbContext.Inscripciones.Add(inscripcion);
        }

        // Notificación Outbox atómica para acuse de recibo de inscripción
        if (_servicioOutbox != null)
        {
            var tutor = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == tutorId);
            var grupo = await _dbContext.GruposScout.FirstOrDefaultAsync(g => g.Id == _tenantContext.CurrentTenantId);
            var nombreGrupo = grupo?.Nombre ?? "Grupo Scout";
            var urlAcceso = "/familias/inscripciones";

            if (tutor != null && !string.IsNullOrWhiteSpace(tutor.Email))
            {
                var contenido = PlantillasCorreo.InscripcionPresentada(
                    educando.NombreCompleto,
                    actividad.Titulo,
                    urlAcceso,
                    nombreGrupo);

                await _servicioOutbox.EncolarMensajeAsync(
                    organizacionId: inscripcion.OrganizacionId,
                    tipoEvento: "InscripcionPresentada",
                    destinatarioEmail: tutor.Email,
                    destinatarioNombre: tutor.NombreCompleto,
                    asunto: contenido.Asunto,
                    cuerpoHtml: contenido.CuerpoHtml,
                    cuerpoTextoPlano: contenido.CuerpoTextoPlano,
                    claveDeduplicacion: $"inscripcion-presentada-{inscripcion.Id}",
                    replyToEmail: grupo?.EmailContacto,
                    remitenteNombre: nombreGrupo,
                    guardarInmediatamente: false);
            }
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Inscripción registrada con éxito: Educando {EducandoId} en Actividad {ActividadId}. Estado: {Estado}. Importe: {Importe:C2}",
            educandoId, actividadId, inscripcion.Estado, inscripcion.ImporteEuros);

        return inscripcion;
    }

    public async Task<IReadOnlyList<InscripcionActividad>> ObtenerInscripcionesFamiliaAsync(Guid tutorId)
    {
        ValidarTenantActivo();

        return await _dbContext.Inscripciones
            .AsNoTracking()
            .Include(i => i.Actividad)
            .Include(i => i.Educando)
            .Where(i => i.TutorLegalId == tutorId)
            .OrderByDescending(i => i.FechaInscripcionUtc)
            .ToListAsync();
    }

    /// <summary>Inscripciones de una actividad limitadas a las secciones del alcance del usuario actual.</summary>
    public async Task<IReadOnlyList<InscripcionActividad>> ObtenerInscripcionesActividadAsync(Guid actividadId)
    {
        ValidarTenantActivo();

        var alcance = await _alcance.ObtenerAlcanceAsync();
        if (!alcance.TieneAccesoKraal)
        {
            return [];
        }

        var query = _dbContext.Inscripciones
            .AsNoTracking()
            .Include(i => i.Educando)
                .ThenInclude(e => e.FichaMedica)
            .Include(i => i.TutorLegal)
            .Include(i => i.RevisadoPorUsuario)
            .Where(i => i.ActividadId == actividadId);

        if (!alcance.EsGlobal)
        {
            query = query.Where(i => i.Educando.RamaActual == alcance.Rama);
        }

        return await query
            .OrderBy(i => i.FechaInscripcionUtc)
            .ToListAsync();
    }

    /// <summary>
    /// Mesa de revisión. El filtro de rama solo lo decide quien tiene alcance global:
    /// un Scouter o Coordinador de sección ve siempre y únicamente su sección, pida lo que pida.
    /// </summary>
    public async Task<IReadOnlyList<InscripcionActividad>> ObtenerInscripcionesMesaKraalAsync(
        RamaScout? ramaFiltro = null,
        Guid? actividadIdFiltro = null,
        EstadoInscripcion? estadoFiltro = null)
    {
        ValidarTenantActivo();

        var alcance = await _alcance.ObtenerAlcanceAsync();
        if (!alcance.TieneAccesoKraal)
        {
            return [];
        }

        var query = _dbContext.Inscripciones
            .AsNoTracking()
            .Include(i => i.Actividad)
            .Include(i => i.Educando)
                .ThenInclude(e => e.FichaMedica)
            .Include(i => i.TutorLegal)
            .Include(i => i.RevisadoPorUsuario)
            .AsQueryable();

        var ramaConsulta = alcance.RamaDeConsulta(ramaFiltro);
        if (ramaConsulta.HasValue)
        {
            query = query.Where(i => i.Educando.RamaActual == ramaConsulta.Value);
        }

        if (actividadIdFiltro.HasValue && actividadIdFiltro.Value != Guid.Empty)
        {
            query = query.Where(i => i.ActividadId == actividadIdFiltro.Value);
        }

        if (estadoFiltro.HasValue)
        {
            query = query.Where(i => i.Estado == estadoFiltro.Value);
        }

        return await query
            .OrderByDescending(i => i.FechaInscripcionUtc)
            .ToListAsync();
    }

    public async Task<InscripcionActividad?> ObtenerPorIdAsync(Guid inscripcionId)
    {
        ValidarTenantActivo();

        return await _dbContext.Inscripciones
            .Include(i => i.Actividad)
            .Include(i => i.Educando)
            .Include(i => i.TutorLegal)
            .Include(i => i.RevisadoPorUsuario)
            .FirstOrDefaultAsync(i => i.Id == inscripcionId);
    }

    public async Task SubirJustificantePagoAsync(
        Guid inscripcionId,
        string nombreArchivoOriginal,
        Stream streamContenido)
    {
        ValidarTenantActivo();

        var inscripcion = await _dbContext.Inscripciones
            .FirstOrDefaultAsync(i => i.Id == inscripcionId);

        if (inscripcion == null)
        {
            throw new InvalidOperationException("La inscripción no existe o no pertenece a su grupo scout.");
        }

        if (inscripcion.Estado == EstadoInscripcion.Cancelada)
        {
            throw new InvalidOperationException("No se puede adjuntar un comprobante de pago a una inscripción que ha sido cancelada.");
        }

        // Si ya tenía un justificante anterior, eliminar el archivo previo
        if (!string.IsNullOrWhiteSpace(inscripcion.RutaArchivoJustificante))
        {
            try
            {
                await _almacenamientoJustificantes.EliminarJustificanteAsync(inscripcion.RutaArchivoJustificante);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar el justificante anterior {Ruta}", inscripcion.RutaArchivoJustificante);
            }
        }

        var rutaRelativa = await _almacenamientoJustificantes.GuardarJustificanteAsync(
            inscripcion.OrganizacionId,
            inscripcion.Id,
            nombreArchivoOriginal,
            streamContenido);

        inscripcion.NombreArchivoJustificante = nombreArchivoOriginal;
        inscripcion.RutaArchivoJustificante = rutaRelativa;
        inscripcion.FechaSubidaJustificanteUtc = DateTime.UtcNow;
        inscripcion.Estado = EstadoInscripcion.EnRevision;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Justificante adjuntado para inscripción {InscripcionId}. Estado actualizado a EnRevision.", inscripcionId);
    }

    public async Task RevisarInscripcionAsync(
        Guid inscripcionId,
        EstadoInscripcion nuevoEstado,
        string? notasKraal,
        Guid scouterId)
    {
        ValidarTenantActivo();

        var inscripcion = await _dbContext.Inscripciones
            .Include(i => i.Educando)
            .FirstOrDefaultAsync(i => i.Id == inscripcionId);

        // Sin alcance sobre la sección del educando se responde igual que si no existiera,
        // para no confirmar la existencia de inscripciones de otras secciones.
        var alcance = await _alcance.ObtenerAlcanceAsync();
        if (inscripcion == null || !alcance.PuedeVer(inscripcion.Educando.RamaActual))
        {
            throw new InvalidOperationException("La inscripción no existe o no pertenece a su grupo scout.");
        }

        inscripcion.Estado = nuevoEstado;
        inscripcion.NotasKraal = notasKraal;
        inscripcion.RevisadoPorUsuarioId = scouterId;
        inscripcion.FechaRevisionUtc = DateTime.UtcNow;

        // Despacho transaccional Outbox según la resolución del Kraal (PA11 / ADR07)
        if (_servicioOutbox != null)
        {
            var tutor = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == inscripcion.TutorLegalId);
            var educando = await _dbContext.Educandos.FirstOrDefaultAsync(e => e.Id == inscripcion.EducandoId);
            var actividad = await _dbContext.Actividades.FirstOrDefaultAsync(a => a.Id == inscripcion.ActividadId);
            var grupo = await _dbContext.GruposScout.FirstOrDefaultAsync(g => g.Id == inscripcion.OrganizacionId);
            var nombreGrupo = grupo?.Nombre ?? "Grupo Scout";
            var urlAcceso = "/familias/inscripciones";

            if (tutor != null && !string.IsNullOrWhiteSpace(tutor.Email) && educando != null && actividad != null)
            {
                if (nuevoEstado == EstadoInscripcion.RequiereSubsanacion)
                {
                    var contenido = PlantillasCorreo.SubsanacionRequerida(
                        educando.NombreCompleto,
                        actividad.Titulo,
                        notasKraal,
                        urlAcceso,
                        nombreGrupo);

                    var claveDeduplicacion = $"subsanacion-{inscripcion.Id}-{inscripcion.FechaRevisionUtc?.Ticks ?? DateTime.UtcNow.Ticks}";

                    await _servicioOutbox.EncolarMensajeAsync(
                        organizacionId: inscripcion.OrganizacionId,
                        tipoEvento: "InscripcionSubsanacionRequerida",
                        destinatarioEmail: tutor.Email,
                        destinatarioNombre: tutor.NombreCompleto,
                        asunto: contenido.Asunto,
                        cuerpoHtml: contenido.CuerpoHtml,
                        cuerpoTextoPlano: contenido.CuerpoTextoPlano,
                        claveDeduplicacion: claveDeduplicacion,
                        replyToEmail: grupo?.EmailContacto,
                        remitenteNombre: nombreGrupo,
                        guardarInmediatamente: false);
                }
                else if (nuevoEstado == EstadoInscripcion.Confirmada)
                {
                    var contenido = PlantillasCorreo.ConfirmacionInscripcion(
                        educando.NombreCompleto,
                        actividad.Titulo,
                        actividad.FechaInicio,
                        actividad.Lugar,
                        urlAcceso,
                        nombreGrupo);

                    var claveDeduplicacion = $"confirmacion-{inscripcion.Id}";

                    await _servicioOutbox.EncolarMensajeAsync(
                        organizacionId: inscripcion.OrganizacionId,
                        tipoEvento: "InscripcionConfirmada",
                        destinatarioEmail: tutor.Email,
                        destinatarioNombre: tutor.NombreCompleto,
                        asunto: contenido.Asunto,
                        cuerpoHtml: contenido.CuerpoHtml,
                        cuerpoTextoPlano: contenido.CuerpoTextoPlano,
                        claveDeduplicacion: claveDeduplicacion,
                        replyToEmail: grupo?.EmailContacto,
                        remitenteNombre: nombreGrupo,
                        guardarInmediatamente: false);
                }
            }
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Inscripción {InscripcionId} revisada por Scouter {ScouterId}: Nuevo estado {Estado}",
            inscripcionId, scouterId, nuevoEstado);
    }

    public async Task CancelarInscripcionAsync(Guid inscripcionId, Guid tutorId)
    {
        ValidarTenantActivo();

        var inscripcion = await _dbContext.Inscripciones
            .FirstOrDefaultAsync(i => i.Id == inscripcionId);

        if (inscripcion == null)
        {
            throw new InvalidOperationException("La inscripción no existe o no pertenece a su grupo scout.");
        }

        if (inscripcion.TutorLegalId != tutorId)
        {
            throw new InvalidOperationException("No dispone de permisos para cancelar esta inscripción.");
        }

        inscripcion.Estado = EstadoInscripcion.Cancelada;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Inscripción {InscripcionId} cancelada por el tutor {TutorId}.", inscripcionId, tutorId);
    }

    private void ValidarTenantActivo()
    {
        if (!_tenantContext.IsResolved || _tenantContext.CurrentTenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Operación no permitida sin una Organización activa.");
        }
    }
}
