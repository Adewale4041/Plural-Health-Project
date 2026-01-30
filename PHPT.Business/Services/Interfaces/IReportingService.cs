using PHPT.Common.DTOs;

namespace PHPT.Business.Services.Interfaces;

public interface IReportingService
{
    Task<FacilityReportDto> GetFacilityReportAsync(Guid facilityId);
    Task<List<StaffPerformanceDto>> GetStaffPerformanceAsync(Guid facilityId, Guid? staffId = null);
    Task<List<AppointmentListDto>> GetAppointmentsByStaffAsync(Guid facilityId, Guid? staffId, DateTime? startDate, DateTime? endDate);
    Task<List<InvoiceDto>> GetInvoicesByStaffAsync(Guid facilityId, Guid? staffId, DateTime? startDate, DateTime? endDate);
}
