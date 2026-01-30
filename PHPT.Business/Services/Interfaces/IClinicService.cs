using PHPT.Common.DTOs;
using PHPT.Common.Models;

namespace PHPT.Business.Services.Interfaces;

public interface IClinicService
{
    Task<ClinicDto> CreateClinicAsync(Guid facilityId, CreateClinicDto dto, Guid createdByUserId);
    Task<ClinicDto> UpdateClinicAsync(Guid clinicId, Guid facilityId, UpdateClinicDto dto, Guid updatedByUserId);
    Task<ClinicDto?> GetClinicByIdAsync(Guid clinicId, Guid facilityId);
    Task<ClinicDto?> GetClinicByCodeAsync(string clinicCode, Guid facilityId);
    Task<ClinicDto?> GetClinicByNameAsync(string clinicName, Guid facilityId);
    Task<PagedResult<ClinicDto>> GetClinicsAsync(Guid facilityId, ClinicFilterDto filter);
    Task<bool> DeactivateClinicAsync(Guid clinicId, Guid facilityId);
    Task<bool> ActivateClinicAsync(Guid clinicId, Guid facilityId);
}
