using Microsoft.EntityFrameworkCore;
using PHPT.Data.Context;
using PHPT.Data.Entities;
using PHPT.Data.Repositories.Interfaces;

namespace PHPT.Data.Repositories.Implementations;

public class FacilityRepository : Repository<Facility>, IFacilityRepository
{
    public FacilityRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Facility?> GetFacilityByCodeAsync(string code)
    {
        return await _dbSet.FirstOrDefaultAsync(f => f.Code == code && f.IsActive);
    }

    public async Task<IEnumerable<Facility>> GetActiveFacilitiesAsync()
    {
        return await _dbSet.Where(f => f.IsActive).OrderBy(f => f.Name).ToListAsync();
    }

    public async Task<bool> FacilityExistsAsync(Guid facilityId)
    {
        return await _dbSet.AnyAsync(f => f.Id == facilityId && f.IsActive);
    }
}
