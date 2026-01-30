using PHPT.Data.Entities;

namespace PHPT.Data.Repositories.Interfaces;

public interface IFacilityRepository : IRepository<Facility>
{
    Task<Facility?> GetFacilityByCodeAsync(string code);
    Task<IEnumerable<Facility>> GetActiveFacilitiesAsync();
    Task<bool> FacilityExistsAsync(Guid facilityId);
}
