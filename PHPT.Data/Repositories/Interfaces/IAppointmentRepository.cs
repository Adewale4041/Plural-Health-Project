using PHPT.Common.Models;
using PHPT.Data.Entities;

namespace PHPT.Data.Repositories.Interfaces;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<PagedResult<Appointment>> GetAppointmentsPagedAsync(
        Guid facilityId,
        DateTime? startDate,
        DateTime? endDate,
        Guid? clinicId,
        string? searchTerm,
        int pageNumber,
        int pageSize,
        bool sortByTimeAscending = true);
        
    Task<Appointment?> GetAppointmentWithDetailsAsync(Guid appointmentId);
    Task<bool> HasOverlappingAppointmentAsync(Guid clinicId, DateTime appointmentDate, TimeSpan appointmentTime, Guid? excludeAppointmentId = null);
}
