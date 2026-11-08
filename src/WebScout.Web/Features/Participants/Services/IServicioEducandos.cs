using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Participants.Domain;

namespace WebScout.Web.Features.Participants.Services;

/// <summary>
/// Contrato del servicio de gestión de educandos y fichas médicas.
/// </summary>
public interface IServicioEducandos
{
    Task<List<Educando>> ObtenerPorTutorAsync(Guid tutorLegalId);
    Task<List<Educando>> ObtenerPorRamaAsync(RamaScout rama);
    Task<Educando?> ObtenerPorIdAsync(Guid educandoId);
    Task<Educando> CrearEducandoAsync(Educando educando, FichaMedica fichaMedica);
    Task ActualizarEducandoAsync(Educando educando);
    Task ActualizarFichaMedicaAsync(FichaMedica fichaMedica);
}

/// <summary>
/// Alias retrocompatible de transición para IServicioEducandos.
/// </summary>
public interface IParticipantService : IServicioEducandos
{
    Task<List<ScoutParticipant>> GetParticipantsByGuardianAsync(Guid guardianId);
    Task<List<ScoutParticipant>> GetParticipantsByBranchAsync(ScoutBranch branch);
    Task<ScoutParticipant?> GetParticipantByIdAsync(Guid participantId);
    Task<ScoutParticipant> CreateParticipantAsync(ScoutParticipant participant, ParticipantHealthRecord healthRecord);
    Task UpdateParticipantAsync(ScoutParticipant participant);
    Task UpdateHealthRecordAsync(ParticipantHealthRecord healthRecord);
}
