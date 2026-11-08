using Microsoft.EntityFrameworkCore;
using WebScout.Web.Features.Identity.Domain;
using WebScout.Web.Features.Participants.Domain;
using WebScout.Web.Infrastructure.Persistence;

namespace WebScout.Web.Features.Participants.Services;

public class ParticipantService : IParticipantService
{
    private readonly WebScoutDbContext _db;

    public ParticipantService(WebScoutDbContext db)
    {
        _db = db;
    }

    public async Task<List<ScoutParticipant>> GetParticipantsByGuardianAsync(Guid guardianId)
    {
        return await _db.Participants
            .Include(p => p.HealthRecord)
            .Where(p => p.LegalGuardianId == guardianId && p.IsActive)
            .OrderBy(p => p.BirthDate)
            .ToListAsync();
    }

    public async Task<List<ScoutParticipant>> GetParticipantsByBranchAsync(ScoutBranch branch)
    {
        return await _db.Participants
            .Include(p => p.HealthRecord)
            .Where(p => p.CurrentBranch == branch && p.IsActive)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<ScoutParticipant?> GetParticipantByIdAsync(Guid participantId)
    {
        return await _db.Participants
            .Include(p => p.HealthRecord)
            .Include(p => p.LegalGuardian)
            .FirstOrDefaultAsync(p => p.Id == participantId);
    }

    public async Task<ScoutParticipant> CreateParticipantAsync(ScoutParticipant participant, ParticipantHealthRecord healthRecord)
    {
        healthRecord.Id = Guid.NewGuid();
        healthRecord.ParticipantId = participant.Id;
        participant.HealthRecord = healthRecord;

        _db.Participants.Add(participant);
        await _db.SaveChangesAsync();
        return participant;
    }

    public async Task UpdateParticipantAsync(ScoutParticipant participant)
    {
        participant.UpdatedAtUtc = DateTime.UtcNow;
        _db.Participants.Update(participant);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateHealthRecordAsync(ParticipantHealthRecord healthRecord)
    {
        healthRecord.LastUpdatedAtUtc = DateTime.UtcNow;
        _db.HealthRecords.Update(healthRecord);
        await _db.SaveChangesAsync();
    }
}
