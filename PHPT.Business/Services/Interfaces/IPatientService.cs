using PHPT.Common.DTOs;
using PHPT.Common.Models;

namespace PHPT.Business.Services.Interfaces;

public interface IPatientService
{
    Task<PatientDto> CreatePatientAsync(Guid facilityId, CreatePatientDto dto, Guid createdByUserId);
    Task<PatientDto?> GetPatientByIdAsync(Guid patientId, Guid facilityId);
    Task<PatientDto?> GetPatientByCodeAsync(string patientCode, Guid facilityId);
    Task<PatientDto?> GetPatientByEmailAsync(string email, Guid facilityId);
    Task<PagedResult<PatientDto>> GetPatientsAsync(Guid facilityId, PatientFilterDto filter);
}
