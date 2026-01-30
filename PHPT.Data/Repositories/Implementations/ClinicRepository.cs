using Microsoft.EntityFrameworkCore;
using PHPT.Data.Context;
using PHPT.Data.Entities;
using PHPT.Data.Repositories.Interfaces;

namespace PHPT.Data.Repositories.Implementations;

public class ClinicRepository : Repository<Clinic>, IClinicRepository
{
    public ClinicRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Clinic?> GetClinicByCodeAsync(string code, Guid facilityId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Code == code && c.FacilityId == facilityId);
    }

    public async Task<IEnumerable<Clinic>> GetClinicsByFacilityAsync(Guid facilityId)
    {
        return await _dbSet
            .Where(c => c.FacilityId == facilityId && c.IsActive)
            .ToListAsync();
    }
}
