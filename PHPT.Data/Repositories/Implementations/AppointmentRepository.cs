using Microsoft.EntityFrameworkCore;
using PHPT.Common.Models;
using PHPT.Data.Context;
using PHPT.Data.Entities;
using PHPT.Data.Repositories.Interfaces;

namespace PHPT.Data.Repositories.Implementations;

public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Appointment>> GetAppointmentsPagedAsync(
        Guid facilityId,
        DateTime? startDate,
        DateTime? endDate,
        Guid? clinicId,
        string? searchTerm,
        int pageNumber,
        int pageSize,
        bool sortByTimeAscending = true)
    {
        var query = _dbSet
            .Include(a => a.Patient)
                .ThenInclude(p => p.Wallet)
            .Include(a => a.Clinic)
            .Include(a => a.Invoice)
            .Where(a => a.FacilityId == facilityId);

        if (startDate.HasValue)
            query = query.Where(a => a.AppointmentDate >= startDate.Value.Date);

        if (endDate.HasValue)
            query = query.Where(a => a.AppointmentDate <= endDate.Value.Date);

        if (clinicId.HasValue)
            query = query.Where(a => a.ClinicId == clinicId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(a =>
                a.Patient.FirstName.ToLower().Contains(searchLower) ||
                a.Patient.LastName.ToLower().Contains(searchLower) ||
                a.Patient.PatientCode.ToLower().Contains(searchLower) ||
                a.Patient.Phone.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();

        query = sortByTimeAscending
            ? query.OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime)
            : query.OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.AppointmentTime);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Appointment>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Appointment?> GetAppointmentWithDetailsAsync(Guid appointmentId)
    {
        return await _dbSet
            .Include(a => a.Patient)
                .ThenInclude(p => p.Wallet)
            .Include(a => a.Clinic)
            .Include(a => a.Invoice)
                .ThenInclude(i => i!.Items)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);
    }

    public async Task<bool> HasOverlappingAppointmentAsync(
        Guid clinicId,
        DateTime appointmentDate,
        TimeSpan appointmentTime,
        Guid? excludeAppointmentId = null)
    {
        var query = _dbSet.Where(a =>
            a.ClinicId == clinicId &&
            a.AppointmentDate.Date == appointmentDate.Date &&
            a.AppointmentTime == appointmentTime &&
            !a.IsDeleted);

        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);

        return await query.AnyAsync();
    }
}
