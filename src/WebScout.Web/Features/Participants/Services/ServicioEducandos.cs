using Microsoft.EntityFrameworkCore;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Identity.Services;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Persistence;

namespace WebScout.Web.Features.Participants.Services;

/// <summary>
/// Implementación del servicio de dominio de educandos y expedientes sanitarios.
/// </summary>
public class ServicioEducandos : IServicioEducandos, IParticipantService
{
    private readonly WebScoutDbContext _db;
    private readonly IServicioAlcanceSeccion _alcance;

    public ServicioEducandos(WebScoutDbContext db, IServicioAlcanceSeccion alcance)
    {
        _db = db;
        _alcance = alcance;
    }

    public async Task<List<Educando>> ObtenerPorTutorAsync(Guid tutorLegalId)
    {
        return await _db.Educandos
            .Include(e => e.FichaMedica)
            .Where(e => e.TutorLegalId == tutorLegalId && e.Activo)
            .OrderBy(e => e.FechaNacimiento)
            .ToListAsync();
    }

    /// <summary>Censo de una rama. Sin alcance sobre esa rama devuelve una lista vacía.</summary>
    public async Task<List<Educando>> ObtenerPorRamaAsync(RamaScout rama)
    {
        var alcance = await _alcance.ObtenerAlcanceAsync();
        if (!alcance.PuedeVer(rama))
        {
            return [];
        }

        return await _db.Educandos
            .Include(e => e.FichaMedica)
            .Include(e => e.TutorLegal)
            .Where(e => e.RamaActual == rama && e.Activo)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombre)
            .ToListAsync();
    }

    /// <summary>
    /// Censo visible para el usuario actual: todas las ramas con alcance global,
    /// solo la propia para un Scouter o Coordinador de sección, nada sin alcance de Kraal.
    /// </summary>
    public async Task<List<Educando>> ObtenerTodosDelTenantAsync()
    {
        var alcance = await _alcance.ObtenerAlcanceAsync();
        if (!alcance.TieneAccesoKraal)
        {
            return [];
        }

        var consulta = _db.Educandos
            .Include(e => e.FichaMedica)
            .Include(e => e.TutorLegal)
            .Where(e => e.Activo);

        if (!alcance.EsGlobal)
        {
            consulta = consulta.Where(e => e.RamaActual == alcance.Rama);
        }

        return await consulta
            .OrderBy(e => e.RamaActual)
            .ThenBy(e => e.Apellidos)
            .ThenBy(e => e.Nombre)
            .ToListAsync();
    }

    public async Task<Educando?> ObtenerPorIdAsync(Guid educandoId)
    {
        return await _db.Educandos
            .Include(e => e.FichaMedica)
            .Include(e => e.TutorLegal)
            .FirstOrDefaultAsync(e => e.Id == educandoId);
    }

    public async Task<Educando> CrearEducandoAsync(Educando educando, FichaMedica fichaMedica)
    {
        fichaMedica.Id = Guid.NewGuid();
        fichaMedica.EducandoId = educando.Id;
        educando.FichaMedica = fichaMedica;

        _db.Educandos.Add(educando);
        await _db.SaveChangesAsync();
        return educando;
    }

    public async Task ActualizarEducandoAsync(Educando educando)
    {
        educando.FechaActualizacionUtc = DateTime.UtcNow;
        _db.Educandos.Update(educando);
        await _db.SaveChangesAsync();
    }

    public async Task ActualizarFichaMedicaAsync(FichaMedica fichaMedica)
    {
        fichaMedica.UltimaActualizacionUtc = DateTime.UtcNow;
        _db.FichasMedicas.Update(fichaMedica);
        await _db.SaveChangesAsync();
    }

    // Métodos retrocompatibles para IParticipantService
    public async Task<List<ScoutParticipant>> GetParticipantsByGuardianAsync(Guid guardianId)
    {
        var list = await ObtenerPorTutorAsync(guardianId);
        return list.Cast<ScoutParticipant>().ToList();
    }

    public async Task<List<ScoutParticipant>> GetParticipantsByBranchAsync(ScoutBranch branch)
    {
        var list = await ObtenerPorRamaAsync((RamaScout)(int)branch);
        return list.Cast<ScoutParticipant>().ToList();
    }

    public async Task<ScoutParticipant?> GetParticipantByIdAsync(Guid participantId)
    {
        var item = await ObtenerPorIdAsync(participantId);
        return (ScoutParticipant?)item;
    }

    public async Task<ScoutParticipant> CreateParticipantAsync(ScoutParticipant participant, ParticipantHealthRecord healthRecord)
    {
        var created = await CrearEducandoAsync(participant, healthRecord);
        return (ScoutParticipant)created;
    }

    public Task UpdateParticipantAsync(ScoutParticipant participant) => ActualizarEducandoAsync(participant);

    public Task UpdateHealthRecordAsync(ParticipantHealthRecord healthRecord) => ActualizarFichaMedicaAsync(healthRecord);
}

/// <summary>
/// Alias retrocompatible de transición para ServicioEducandos.
/// </summary>
public class ParticipantService : ServicioEducandos
{
    public ParticipantService(WebScoutDbContext db, IServicioAlcanceSeccion alcance) : base(db, alcance)
    {
    }
}
