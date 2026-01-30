using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PHPT.Business.Services.Interfaces;
using PHPT.Common.Constants;
using PHPT.Common.DTOs;
using PHPT.Common.Models;
using PHPT.Data.Entities;
using PHPT.Data.UnitOfWork;

namespace PHPT.Business.Services.Implementations;

public class PatientService : IPatientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PatientService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public PatientService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, ILogger<PatientService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<PatientDto> CreatePatientAsync(Guid facilityId, CreatePatientDto dto, Guid createdByUserId)
    {
        try
        {
            _logger.LogInformation("Creating new patient for facility {FacilityId}", facilityId);

            var user = await _userManager.FindByIdAsync(createdByUserId.ToString());
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Verify facility exists
            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }

            // Check if patient with same email already exists in this facility
            var existingPatient = await _unitOfWork.Patients
                .FirstOrDefaultAsync(p => p.Email == dto.Email && p.FacilityId == facilityId);
            
            if (existingPatient != null)
            {
                throw new InvalidOperationException($"A patient with email {dto.Email} already exists in this facility");
            }

            // Generate unique patient code
            var patientCode = await GenerateUniquePatientCodeAsync(facilityId);

            // Create patient entity
            var patient = new Patient
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PatientCode = patientCode,
                Phone = dto.Phone,
                Email = dto.Email,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Address = dto.Address,
                FacilityId = facilityId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Patients.AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Patient created successfully: {PatientCode}", patientCode);

            // Create patient wallet
            var wallet = new PatientWallet
            {
                PatientId = patient.Id,
                Balance = dto.InitialWalletBalance,
                Currency = AppConstants.DefaultCurrency,
                LastTransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Wallets.AddAsync(wallet);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Wallet created for patient {PatientCode} with initial balance {Balance}", 
                patientCode, dto.InitialWalletBalance);

            // Return patient DTO
            return MapToDto(patient, facility.Name, wallet.Balance, wallet.Currency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            throw;
        }
    }

    public async Task<PatientDto?> GetPatientByIdAsync(Guid patientId, Guid facilityId)
    {
        try
        {
            _logger.LogInformation("Fetching patient {PatientId} for facility {FacilityId}", patientId, facilityId);

            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }

            var patient = await _unitOfWork.Patients.GetByIdAsync(patientId);
            
            if (patient == null || patient.FacilityId != facilityId)
            {
                _logger.LogWarning("Patient {PatientId} not found or does not belong to facility {FacilityId}", 
                    patientId, facilityId);
                return null;
            }

            var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.PatientId == patientId);

            return MapToDto(patient, facility?.Name ?? string.Empty, wallet?.Balance ?? 0, wallet?.Currency ?? AppConstants.DefaultCurrency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patient {PatientId}", patientId);
            throw;
        }
    }

    public async Task<PatientDto?> GetPatientByCodeAsync(string patientCode, Guid facilityId)
    {
        try
        {
            _logger.LogInformation("Fetching patient by code {PatientCode} for facility {FacilityId}", 
                patientCode, facilityId);

            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }

            var patient = await _unitOfWork.Patients
                .FirstOrDefaultAsync(p => p.PatientCode == patientCode && p.FacilityId == facilityId);
            
            if (patient == null)
            {
                _logger.LogWarning("Patient with code {PatientCode} not found in facility {FacilityId}", 
                    patientCode, facilityId);
                return null;
            }

            var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.PatientId == patient.Id);

            return MapToDto(patient, facility?.Name ?? string.Empty, wallet?.Balance ?? 0, wallet?.Currency ?? AppConstants.DefaultCurrency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patient by code {PatientCode}", patientCode);
            throw;
        }
    }

    public async Task<PatientDto?> GetPatientByEmailAsync(string email, Guid facilityId)
    {
        try
        {
            _logger.LogInformation("Fetching patient by email {Email} for facility {FacilityId}", 
                email, facilityId);

            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }

            var patient = await _unitOfWork.Patients
                .FirstOrDefaultAsync(p => p.Email == email && p.FacilityId == facilityId);
            
            if (patient == null)
            {
                _logger.LogWarning("Patient with email {Email} not found in facility {FacilityId}", 
                    email, facilityId);
                return null;
            }

            var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.PatientId == patient.Id);

            return MapToDto(patient, facility?.Name ?? string.Empty, wallet?.Balance ?? 0, wallet?.Currency ?? AppConstants.DefaultCurrency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patient by email {Email}", email);
            throw;
        }
    }

    public async Task<PagedResult<PatientDto>> GetPatientsAsync(Guid facilityId, PatientFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Fetching patients for facility {FacilityId} with filters", facilityId);

            // Verify facility exists
            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }

            // Build query
            var query = _unitOfWork.Patients.Query(p => p.FacilityId == facilityId);

            // Apply date filter (default to today if not specified)
            var startDate = filter.StartDate ?? DateTime.Today;
            var endDate = filter.EndDate ?? DateTime.Today.AddDays(1).AddTicks(-1);
            
            query = query.Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);

            // Apply gender filter
            if (!string.IsNullOrWhiteSpace(filter.Gender))
            {
                query = query.Where(p => p.Gender == filter.Gender);
            }

            // Apply search filter (name, code, or phone)
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.FirstName.ToLower().Contains(searchTerm) ||
                    p.LastName.ToLower().Contains(searchTerm) ||
                    p.PatientCode.ToLower().Contains(searchTerm) ||
                    p.Phone.Contains(searchTerm));
                
                _logger.LogInformation("Applied search filter: {SearchTerm}", filter.SearchTerm);
            }

            // Get total count
            var totalCount = await _unitOfWork.Patients.CountAsync(query);

            // Apply sorting
            query = filter.SortByNameAscending
                ? query.OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
                : query.OrderByDescending(p => p.FirstName).ThenByDescending(p => p.LastName);

            // Apply pagination
            var pageSize = Math.Min(filter.PageSize, 100); // Max 100 items per page
            var skip = (filter.PageNumber - 1) * pageSize;
            
            var patients = await _unitOfWork.Patients.GetPagedAsync(query, skip, pageSize);

            _logger.LogInformation("Found {Count} patients for facility {FacilityId}", 
                totalCount, facilityId);

            // Get facility name
            var facilityName = facility?.Name ?? string.Empty;

            // Get all wallets for these patients
            var patientIds = patients.Select(p => p.Id).ToList();
            var wallets = await _unitOfWork.Wallets.FindAsync(w => patientIds.Contains(w.PatientId));
            var walletDict = wallets.ToDictionary(w => w.PatientId);

            // Map to DTOs
            var patientDtos = patients.Select(p =>
            {
                var wallet = walletDict.ContainsKey(p.Id) ? walletDict[p.Id] : null;
                return MapToDto(p, facilityName, wallet?.Balance ?? 0, wallet?.Currency ?? AppConstants.DefaultCurrency);
            }).ToList();

            return new PagedResult<PatientDto>
            {
                Items = patientDtos,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patients for facility {FacilityId}", facilityId);
            throw;
        }
    }

    private async Task<string> GenerateUniquePatientCodeAsync(Guid facilityId)
    {
        // Get the count of patients in this facility
        var count = await _unitOfWork.Patients.CountAsync(p => p.FacilityId == facilityId);
        var nextNumber = count + 1;

        string patientCode;
        bool isUnique;

        do
        {
            // Generate code in format: PAT followed by 6 digits
            patientCode = $"PAT{nextNumber:D6}";
            
            // Check if code already exists
            var existing = await _unitOfWork.Patients
                .FirstOrDefaultAsync(p => p.PatientCode == patientCode);
            
            isUnique = existing == null;
            
            if (!isUnique)
            {
                nextNumber++;
            }
        }
        while (!isUnique);

        return patientCode;
    }

    private PatientDto MapToDto(Patient patient, string facilityName, decimal walletBalance, string currency)
    {
        var age = DateTime.Today.Year - patient.DateOfBirth.Year;
        if (patient.DateOfBirth.Date > DateTime.Today.AddYears(-age))
        {
            age--;
        }

        return new PatientDto
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            FullName = $"{patient.FirstName} {patient.LastName}",
            PatientCode = patient.PatientCode,
            Phone = patient.Phone,
            Email = patient.Email,
            DateOfBirth = patient.DateOfBirth,
            Age = age,
            Gender = patient.Gender,
            Address = patient.Address,
            FacilityId = patient.FacilityId,
            FacilityName = facilityName,
            WalletBalance = walletBalance,
            Currency = currency,
            WalletBalanceFormatted = $"{currency} {walletBalance:N2}",
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt
        };
    }
}
