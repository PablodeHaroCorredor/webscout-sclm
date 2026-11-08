using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Participants.Domain;

namespace WebScout.Web.Features.Participants.Services;

public interface IParticipantService
{
    Task<List<ScoutParticipant>> GetParticipantsByGuardianAsync(Guid guardianId);
    Task<List<ScoutParticipant>> GetParticipantsByBranchAsync(ScoutBranch branch);
    Task<ScoutParticipant?> GetParticipantByIdAsync(Guid participantId);
    Task<ScoutParticipant> CreateParticipantAsync(ScoutParticipant participant, ParticipantHealthRecord healthRecord);
    Task UpdateParticipantAsync(ScoutParticipant participant);
    Task UpdateHealthRecordAsync(ParticipantHealthRecord healthRecord);
}
