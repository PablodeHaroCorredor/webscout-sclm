using Microsoft.EntityFrameworkCore;
using WebScout.Web.Features.Activities.Domain;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Infrastructure.Persistence;
using WebScout.Web.Infrastructure.Tenancy;

namespace WebScout.Web.Features.Activities.Services;

/// <summary>
/// Implementación del servicio de gestión y consulta de actividades y campamentos scout.
/// Aplica filtros por inquilino y control de visibilidad por ramas de los educandos de cada familia.
/// </summary>
public class ServicioActividades : IServicioActividades
{
    private readonly WebScoutDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ServicioActividades(WebScoutDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<ActividadScout>> ObtenerActividadesKraalAsync(
        EstadoActividad? estado = null,
        RamaScout? rama = null)
    {
        var query = _db.Actividades
            .Include(a => a.CreadoPorUsuario)
            .AsNoTracking()
            .AsQueryable();

        if (estado.HasValue)
        {
            query = query.Where(a => a.Estado == estado.Value);
        }

        var list = await query
            .OrderByDescending(a => a.FechaInicio)
            .ToListAsync();

        if (rama.HasValue && rama.Value != RamaScout.Ninguna)
        {
            list = list.Where(a => a.AplicaARama(rama.Value)).ToList();
        }

        return list;
    }

    public async Task<IReadOnlyList<ActividadScout>> ObtenerActividadesParaFamiliaAsync(Guid tutorId)
    {
        // 1. Identificar las ramas en las que la familia tiene educandos activos
        var ramasFamilia = await _db.Educandos
            .AsNoTracking()
            .Where(e => e.TutorLegalId == tutorId && e.Activo)
            .Select(e => e.RamaActual)
            .Distinct()
            .ToListAsync();

        // 2. Estados permitidos para consulta pública familiar
        var estadosVisibles = new[]
        {
            EstadoActividad.Publicada,
            EstadoActividad.InscripcionesCerradas,
            EstadoActividad.EnCurso
        };

        var actividades = await _db.Actividades
            .AsNoTracking()
            .Where(a => estadosVisibles.Contains(a.Estado))
            .OrderBy(a => a.FechaInicio)
            .ToListAsync();

        // 3. Filtrar: aplica si está dirigida al grupo entero (0 ramas especificadas)
        // o si coincide con alguna rama de los educandos de esta familia
        var actividadesVisibles = actividades
            .Where(a => a.RamasDestinatarias.Count == 0 || a.RamasDestinatarias.Any(r => ramasFamilia.Contains(r)))
            .ToList();

        return actividadesVisibles;
    }

    public async Task<ActividadScout?> ObtenerPorIdAsync(Guid actividadId)
    {
        return await _db.Actividades
            .Include(a => a.CreadoPorUsuario)
            .FirstOrDefaultAsync(a => a.Id == actividadId);
    }

    public async Task<ActividadScout> CrearActividadAsync(ActividadScout actividad, Guid? scouterId = null)
    {
        if (!_tenantContext.IsResolved || _tenantContext.CurrentTenantId == Guid.Empty)
        {
            throw new InvalidOperationException("No hay una organización o grupo scout activo en el contexto actual.");
        }

        var orgId = _tenantContext.CurrentTenantId;

        if (actividad.FechaFin < actividad.FechaInicio)
        {
            throw new InvalidOperationException("La fecha de fin no puede ser anterior a la fecha de inicio de la actividad.");
        }

        if (actividad.FechaLimiteInscripcion.HasValue && actividad.FechaLimiteInscripcion.Value > actividad.FechaInicio)
        {
            throw new InvalidOperationException("La fecha límite de inscripción no puede ser posterior a la fecha de inicio.");
        }

        if (actividad.Precio < 0)
        {
            throw new InvalidOperationException("El precio en euros no puede ser negativo.");
        }

        actividad.OrganizacionId = orgId;
        actividad.CreadoPorUsuarioId = scouterId;
        actividad.FechaCreacionUtc = DateTime.UtcNow;

        _db.Actividades.Add(actividad);
        await _db.SaveChangesAsync();

        return actividad;
    }

    public async Task<ActividadScout> ActualizarActividadAsync(ActividadScout actividad)
    {
        var existente = await _db.Actividades.FirstOrDefaultAsync(a => a.Id == actividad.Id)
            ?? throw new InvalidOperationException($"No se encontró la actividad con ID {actividad.Id}.");

        if (actividad.FechaFin < actividad.FechaInicio)
        {
            throw new InvalidOperationException("La fecha de fin no puede ser anterior a la fecha de inicio de la actividad.");
        }

        if (actividad.FechaLimiteInscripcion.HasValue && actividad.FechaLimiteInscripcion.Value > actividad.FechaInicio)
        {
            throw new InvalidOperationException("La fecha límite de inscripción no puede ser posterior a la fecha de inicio.");
        }

        if (actividad.Precio < 0)
        {
            throw new InvalidOperationException("El precio en euros no puede ser negativo.");
        }

        existente.Titulo = actividad.Titulo;
        existente.Descripcion = actividad.Descripcion;
        existente.Tipo = actividad.Tipo;
        existente.Estado = actividad.Estado;
        existente.RamasDestinatarias = [.. actividad.RamasDestinatarias];
        existente.Lugar = actividad.Lugar;
        existente.PuntoEncuentroSalida = actividad.PuntoEncuentroSalida;
        existente.PuntoEncuentroRegreso = actividad.PuntoEncuentroRegreso;
        existente.FechaInicio = actividad.FechaInicio;
        existente.FechaFin = actividad.FechaFin;
        existente.FechaLimiteInscripcion = actividad.FechaLimiteInscripcion;
        existente.Precio = actividad.Precio;
        existente.IbanCuentaBancaria = actividad.IbanCuentaBancaria;
        existente.ConceptoPagoSugerido = actividad.ConceptoPagoSugerido;
        existente.MaterialRequerido = actividad.MaterialRequerido;
        existente.ObservacionesSanitarias = actividad.ObservacionesSanitarias;
        existente.FechaModificacionUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return existente;
    }

    public async Task CambiarEstadoActividadAsync(Guid actividadId, EstadoActividad nuevoEstado)
    {
        var existente = await _db.Actividades.FirstOrDefaultAsync(a => a.Id == actividadId)
            ?? throw new InvalidOperationException($"No se encontró la actividad con ID {actividadId}.");

        existente.Estado = nuevoEstado;
        existente.FechaModificacionUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task EliminarActividadAsync(Guid actividadId)
    {
        var existente = await _db.Actividades.FirstOrDefaultAsync(a => a.Id == actividadId)
            ?? throw new InvalidOperationException($"No se encontró la actividad con ID {actividadId}.");

        if (existente.Estado != EstadoActividad.Borrador && existente.Estado != EstadoActividad.Cancelada)
        {
            throw new InvalidOperationException("Solo se pueden eliminar actividades en estado Borrador o Cancelada.");
        }

        _db.Actividades.Remove(existente);
        await _db.SaveChangesAsync();
    }
}
