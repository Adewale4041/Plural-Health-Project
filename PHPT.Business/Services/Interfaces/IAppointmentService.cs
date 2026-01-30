using PHPT.Common.DTOs;
using PHPT.Common.Models;

namespace PHPT.Business.Services.Interfaces;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentListDto>> GetAppointmentsAsync(Guid facilityId, AppointmentFilterDto filter);
    Task<AppointmentDetailsDto?> GetAppointmentByIdAsync(Guid appointmentId, Guid facilityId);
    Task<AppointmentDetailsDto> CreateAppointmentAsync(Guid facilityId, CreateAppointmentDto dto);
    Task UpdateAppointmentStatusAsync(Guid appointmentId, Guid facilityId);
}
