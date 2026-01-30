using Microsoft.Extensions.Logging;
using PHPT.Business.Services.Interfaces;
using PHPT.Common.DTOs;
using PHPT.Common.Enums;
using PHPT.Common.Models;
using PHPT.Data.Entities;
using PHPT.Data.UnitOfWork;

namespace PHPT.Business.Services.Implementations;

public class ClinicService : IClinicService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClinicService> _logger;

    public ClinicService(IUnitOfWork unitOfWork, ILogger<ClinicService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ClinicDto> CreateClinicAsync(Guid facilityId, CreateClinicDto dto, Guid createdByUserId)
    {
        try
        {
            _logger.LogInformation("Creating new clinic {ClinicCode} for facility {FacilityId}", 
                dto.Code, facilityId);

            // Verify facility exists
            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }

            // Check if clinic code already exists in this facility
            var existingClinicByCode = await _unitOfWork.Clinics
                .FirstOrDefaultAsync(c => c.Code == dto.Code && c.FacilityId == facilityId);
            
            if (existingClinicByCode != null)
            {
                throw new InvalidOperationException($"A clinic with code '{dto.Code}' already exists in this facility");
            }

            // Check if clinic name already exists in this facility
            var existingClinicByName = await _unitOfWork.Clinics
                .FirstOrDefaultAsync(c => c.Name == dto.Name && c.FacilityId == facilityId);
            
            if (existingClinicByName != null)
            {
                throw new InvalidOperationException($"A clinic with name '{dto.Name}' already exists in this facility");
            }

            // Create clinic entity
            var clinic = new Clinic
            {
                Name = dto.Name,
                Code = dto.Code.ToUpper(), // Store codes in uppercase
                Description = dto.Description,
                FacilityId = facilityId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Clinics.AddAsync(clinic);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Clinic {ClinicCode} created successfully with ID {ClinicId}", 
                clinic.Code, clinic.Id);

            return await MapToDtoAsync(clinic, facility.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating clinic {ClinicCode}", dto.Code);
            throw;
        }
    }

    public async Task<ClinicDto> UpdateClinicAsync(Guid clinicId, Guid facilityId, UpdateClinicDto dto, Guid updatedByUserId)
    {
        try
        {
            _logger.LogInformation("Updating clinic {ClinicId} for facility {FacilityId}", 
                clinicId, facilityId);

            var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId);
            
            if (clinic == null || clinic.FacilityId != facilityId)
            {
                throw new InvalidOperationException("Clinic not found or does not belong to your facility");
            }

            // Check if new name conflicts with another clinic
            var existingClinicByName = await _unitOfWork.Clinics
                .FirstOrDefaultAsync(c => c.Name == dto.Name && c.FacilityId == facilityId && c.Id != clinicId);
            
            if (existingClinicByName != null)
            {
                throw new InvalidOperationException($"A clinic with name '{dto.Name}' already exists in this facility");
            }

            // Update clinic
            clinic.Name = dto.Name;
            clinic.Description = dto.Description;
            clinic.IsActive = dto.IsActive;
            clinic.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Clinics.Update(clinic);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Clinic {ClinicId} updated successfully", clinicId);

            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            return await MapToDtoAsync(clinic, facility?.Name ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating clinic {ClinicId}", clinicId);
            throw;
        }
    }

    public async Task<ClinicDto?> GetClinicByIdAsync(Guid clinicId, Guid facilityId)
    {
        try
        {
            _logger.LogInformation("Fetching clinic {ClinicId} for facility {FacilityId}", 
                clinicId, facilityId);

            var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId);
            
            if (clinic == null || clinic.FacilityId != facilityId)
            {
                _logger.LogWarning("Clinic {ClinicId} not found or does not belong to facility {FacilityId}", 
                    clinicId, facilityId);
                return null;
            }

            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            return await MapToDtoAsync(clinic, facility?.Name ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching clinic {ClinicId}", clinicId);
            throw;
        }
    }

    public async Task<ClinicDto?> GetClinicByCodeAsync(string clinicCode, Guid facilityId)
    {
        try
        {
            _logger.LogInformation("Fetching clinic by code {ClinicCode} for facility {FacilityId}", 
                clinicCode, facilityId);

            var clinic = await _unitOfWork.Clinics
                .FirstOrDefaultAsync(c => c.Code == clinicCode.ToUpper() && c.FacilityId == facilityId);
            
            if (clinic == null)
            {
                _logger.LogWarning("Clinic with code {ClinicCode} not found in facility {FacilityId}", 
                    clinicCode, facilityId);
                return null;
            }

            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            return await MapToDtoAsync(clinic, facility?.Name ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching clinic by code {ClinicCode}", clinicCode);
            throw;
        }
    }

    public async Task<ClinicDto?> GetClinicByNameAsync(string clinicName, Guid facilityId)
    {
        try
        {
            _logger.LogInformation("Fetching clinic by name '{ClinicName}' for facility {FacilityId}", 
                clinicName, facilityId);

            var clinic = await _unitOfWork.Clinics
                .FirstOrDefaultAsync(c => c.Name.ToLower() == clinicName.ToLower() && c.FacilityId == facilityId);
            
            if (clinic == null)
            {
                _logger.LogWarning("Clinic with name '{ClinicName}' not found in facility {FacilityId}", 
                    clinicName, facilityId);
                return null;
            }

            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            return await MapToDtoAsync(clinic, facility?.Name ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching clinic by name '{ClinicName}'", clinicName);
            throw;
        }
    }

    public async Task<PagedResult<ClinicDto>> GetClinicsAsync(Guid facilityId, ClinicFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Fetching clinics for facility {FacilityId} with filters", facilityId);

            // Build query
            var query = _unitOfWork.Clinics.Query(c => c.FacilityId == facilityId);

            // Apply active/inactive filter
            if (filter.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == filter.IsActive.Value);
                _logger.LogInformation("Applied IsActive filter: {IsActive}", filter.IsActive.Value);
            }

            // Apply search filter (name or code)
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchTerm) ||
                    c.Code.ToLower().Contains(searchTerm) ||
                    c.Description.ToLower().Contains(searchTerm));
                
                _logger.LogInformation("Applied search filter: {SearchTerm}", filter.SearchTerm);
            }

            // Get total count
            var totalCount = await _unitOfWork.Clinics.CountAsync(query);

            // Apply sorting
            query = filter.SortByNameAscending
                ? query.OrderBy(c => c.Name)
                : query.OrderByDescending(c => c.Name);

            // Apply pagination
            var pageSize = Math.Min(filter.PageSize, 100); // Max 100 items per page
            var skip = (filter.PageNumber - 1) * pageSize;
            
            var clinics = await _unitOfWork.Clinics.GetPagedAsync(query, skip, pageSize);

            _logger.LogInformation("Found {Count} clinic(s) for facility {FacilityId}", 
                totalCount, facilityId);

            // Get facility name
            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            var facilityName = facility?.Name ?? string.Empty;

            // Map to DTOs
            var clinicDtos = new List<ClinicDto>();
            foreach (var clinic in clinics)
            {
                clinicDtos.Add(await MapToDtoAsync(clinic, facilityName));
            }

            return new PagedResult<ClinicDto>
            {
                Items = clinicDtos,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching clinics for facility {FacilityId}", facilityId);
            throw;
        }
    }

    public async Task<bool> DeactivateClinicAsync(Guid clinicId, Guid facilityId)
    {
        try
        {
            _logger.LogInformation("Deactivating clinic {ClinicId} in facility {FacilityId}", 
                clinicId, facilityId);

            var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId);
            
            if (clinic == null || clinic.FacilityId != facilityId)
            {
                throw new InvalidOperationException("Clinic not found or does not belong to your facility");
            }

            if (!clinic.IsActive)
            {
                throw new InvalidOperationException("Clinic is already deactivated");
            }

            // Check if clinic has active appointments
            var hasActiveAppointments = await _unitOfWork.Appointments
                .AnyAsync(a => a.ClinicId == clinicId && 
                              (a.Status == AppointmentStatus.Scheduled || 
                               a.Status == AppointmentStatus.AwaitingVitals ||
                               a.Status == AppointmentStatus.InProgress));

            if (hasActiveAppointments)
            {
                throw new InvalidOperationException(
                    "Cannot deactivate clinic with active appointments. Please complete or cancel all active appointments first.");
            }

            clinic.IsActive = false;
            clinic.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Clinics.Update(clinic);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Clinic {ClinicId} deactivated successfully", clinicId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating clinic {ClinicId}", clinicId);
            throw;
        }
    }

    public async Task<bool> ActivateClinicAsync(Guid clinicId, Guid facilityId)
    {
        try
        {
            _logger.LogInformation("Activating clinic {ClinicId} in facility {FacilityId}", 
                clinicId, facilityId);

            var clinic = await _unitOfWork.Clinics.GetByIdAsync(clinicId);
            
            if (clinic == null || clinic.FacilityId != facilityId)
            {
                throw new InvalidOperationException("Clinic not found or does not belong to your facility");
            }

            if (clinic.IsActive)
            {
                throw new InvalidOperationException("Clinic is already active");
            }

            clinic.IsActive = true;
            clinic.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Clinics.Update(clinic);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Clinic {ClinicId} activated successfully", clinicId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating clinic {ClinicId}", clinicId);
            throw;
        }
    }

    private async Task<ClinicDto> MapToDtoAsync(Clinic clinic, string facilityName)
    {
        // Get appointment statistics
        var totalAppointments = await _unitOfWork.Appointments
            .CountAsync(a => a.ClinicId == clinic.Id);

        var activeAppointments = await _unitOfWork.Appointments
            .CountAsync(a => a.ClinicId == clinic.Id && 
                           (a.Status == AppointmentStatus.Scheduled || 
                            a.Status == AppointmentStatus.AwaitingVitals ||
                            a.Status == AppointmentStatus.InProgress));

        return new ClinicDto
        {
            Id = clinic.Id,
            Name = clinic.Name,
            Code = clinic.Code,
            Description = clinic.Description,
            FacilityId = clinic.FacilityId,
            FacilityName = facilityName,
            IsActive = clinic.IsActive,
            TotalAppointments = totalAppointments,
            ActiveAppointments = activeAppointments,
            CreatedAt = clinic.CreatedAt,
            UpdatedAt = clinic.UpdatedAt
        };
    }
}
