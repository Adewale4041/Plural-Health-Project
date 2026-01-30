using PHPT.Data.Entities;

namespace PHPT.Data.Repositories.Interfaces;

public interface IClinicRepository : IRepository<Clinic>
{
    Task<Clinic?> GetClinicByCodeAsync(string code, Guid facilityId);
    Task<IEnumerable<Clinic>> GetClinicsByFacilityAsync(Guid facilityId);
}
