using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PHPT.Business.Services.Interfaces;
using PHPT.Common.Constants;
using PHPT.Common.DTOs;
using PHPT.Common.Enums;
using PHPT.Common.Models;
using PHPT.Data.Entities;
using PHPT.Data.UnitOfWork;

namespace PHPT.Business.Services.Implementations;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(IUnitOfWork unitOfWork, ILogger<AppointmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResult<AppointmentListDto>> GetAppointmentsAsync(Guid facilityId, AppointmentFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Loading appointments for facility {FacilityId} with filters: {@Filter}", facilityId, filter);

            // Verify facility exists
            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }


            var startDate = filter.StartDate ?? DateTime.Today;
            var endDate = filter.EndDate ?? DateTime.Today;

            if (filter.PageSize > AppConstants.MaxPageSize)
                filter.PageSize = AppConstants.MaxPageSize;

            var result = await _unitOfWork.Appointments.GetAppointmentsPagedAsync(
                facilityId,
                startDate,
                endDate,
                filter.ClinicId,
                filter.SearchTerm,
                filter.PageNumber,
                filter.PageSize,
                filter.SortByTimeAscending);

            var dtos = result.Items.Select(a => new AppointmentListDto
            {
                Id = a.Id,
                PatientName = a.Patient.FullName,
                PatientCode = a.Patient.PatientCode,
                PatientPhone = a.Patient.Phone,
                AppointmentDateTime = a.AppointmentDateTime,
                AppointmentTime = a.AppointmentTime.ToString(@"hh\:mm"),
                Status = a.Status,
                StatusDisplay = a.Status.ToString(),
                ClinicName = a.Clinic.Name,
                WalletBalance = a.Patient.Wallet?.Balance ?? 0,
                Currency = a.Patient.Wallet?.Currency ?? AppConstants.DefaultCurrency,
                WalletBalanceFormatted = $"{a.Patient.Wallet?.Currency ?? AppConstants.DefaultCurrency} {a.Patient.Wallet?.Balance ?? 0:N2}",
                HasInvoice = a.Invoice != null,
                InvoiceId = a.Invoice?.Id
            }).ToList();

            _logger.LogInformation("Successfully loaded {Count} appointments for facility {FacilityId}", dtos.Count, facilityId);

            return new PagedResult<AppointmentListDto>
            {
                Items = dtos,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading appointments for facility {FacilityId}", facilityId);
            throw;
        }
    }

    public async Task<AppointmentDetailsDto?> GetAppointmentByIdAsync(Guid appointmentId, Guid facilityId)
    {
        try
        {
            // Verify facility exists
            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }

            var appointment = await _unitOfWork.Appointments.GetAppointmentWithDetailsAsync(appointmentId);
            
            if (appointment == null || appointment.FacilityId != facilityId)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found or doesn't belong to facility {FacilityId}", appointmentId, facilityId);
                return null;
            }

            var dto = new AppointmentDetailsDto
            {
                Id = appointment.Id,
                PatientId = appointment.PatientId,
                PatientName = appointment.Patient.FullName,
                PatientCode = appointment.Patient.PatientCode,
                ClinicId = appointment.ClinicId,
                ClinicName = appointment.Clinic.Name,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                AppointmentDateTime = appointment.AppointmentDateTime,
                AppointmentType = appointment.AppointmentType,
                Status = appointment.Status,
                StatusDisplay = appointment.Status.ToString(),
                Notes = appointment.Notes,
                WalletBalance = appointment.Patient.Wallet?.Balance ?? 0,
                Currency = appointment.Patient.Wallet?.Currency ?? AppConstants.DefaultCurrency
            };

            if (appointment.Invoice != null)
            {
                dto.Invoice = new InvoiceDto
                {
                    Id = appointment.Invoice.Id,
                    InvoiceNumber = appointment.Invoice.InvoiceNumber,
                    PatientId = appointment.Invoice.PatientId,
                    PatientName = appointment.Patient.FullName,
                    AppointmentId = appointment.Invoice.AppointmentId,
                    Subtotal = appointment.Invoice.Subtotal,
                    DiscountAmount = appointment.Invoice.DiscountAmount,
                    DiscountPercentage = appointment.Invoice.DiscountPercentage,
                    TotalAmount = appointment.Invoice.TotalAmount,
                    Status = appointment.Invoice.Status,
                    StatusDisplay = appointment.Invoice.Status.ToString(),
                    PaidAt = appointment.Invoice.PaidAt,
                    Items = appointment.Invoice.Items.Select(i => new InvoiceItemDto
                    {
                        Id = i.Id,
                        ServiceName = i.ServiceName,
                        Description = i.Description,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.TotalPrice
                    }).ToList()
                };
            }

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task<AppointmentDetailsDto> CreateAppointmentAsync(Guid facilityId, CreateAppointmentDto dto)
    {
        try
        {
            _logger.LogInformation("Creating appointment for patient {PatientId} at facility {FacilityId}", dto.PatientId, facilityId);

            // Verify facility exists
            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }

            if (!facility.IsActive)
            {
                throw new InvalidOperationException("Facility is Inactive");
            }

            var patient = await _unitOfWork.Patients.GetByIdAsync(dto.PatientId);
            if (patient == null || patient.FacilityId != facilityId)
            {
                throw new InvalidOperationException("Patient not found or doesn't belong to this facility");
            }

            var clinic = await _unitOfWork.Clinics.GetByIdAsync(dto.ClinicId);
            if (clinic == null || clinic.FacilityId != facilityId || !clinic.IsActive)
            {
                throw new InvalidOperationException("Clinic not found, inactive, or doesn't belong to this facility");
            }

            // Validate appointment date and time
            var now = DateTime.Now;
            var appointmentDateTime = dto.AppointmentDate.Date.Add(dto.AppointmentTime);

            if (appointmentDateTime <= now)
            {
                throw new InvalidOperationException("Appointment date and time must be in the future");
            }

            var hasOverlap = await _unitOfWork.Appointments.HasOverlappingAppointmentAsync(
                dto.ClinicId,
                dto.AppointmentDate,
                dto.AppointmentTime);

            if (hasOverlap)
            {
                throw new InvalidOperationException("An appointment already exists at this time slot");
            }

            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                ClinicId = dto.ClinicId,
                AppointmentDate = dto.AppointmentDate,
                AppointmentTime = dto.AppointmentTime,
                AppointmentType = dto.AppointmentType,
                Notes = dto.Notes,
                Status = AppointmentStatus.Scheduled,
                FacilityId = facilityId
            };

            await _unitOfWork.Appointments.AddAsync(appointment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully created appointment {AppointmentId} for patient {PatientId}", appointment.Id, dto.PatientId);

            return (await GetAppointmentByIdAsync(appointment.Id, facilityId))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment for patient {PatientId}", dto.PatientId);
            throw;
        }
    }

    public async Task UpdateAppointmentStatusAsync(Guid appointmentId, Guid facilityId)
    {
        try
        {
            _logger.LogInformation("Updating appointment {AppointmentId} status to AwaitingVitals", appointmentId);

            // Verify facility exists
            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }


            var appointment = await _unitOfWork.Appointments.GetAppointmentWithDetailsAsync(appointmentId);
            
            if (appointment == null || appointment.FacilityId != facilityId)
            {
                throw new InvalidOperationException("Appointment not found or doesn't belong to this facility");
            }

            if (appointment.Status != AppointmentStatus.Paid)
            {
                throw new InvalidOperationException("Appointment must be in Paid status before transitioning to AwaitingVitals");
            }

            appointment.Status = AppointmentStatus.AwaitingVitals;
            appointment.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated appointment {AppointmentId} to AwaitingVitals status", appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {AppointmentId} status", appointmentId);
            throw;
        }
    }
}
