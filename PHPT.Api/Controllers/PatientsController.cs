using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHPT.Business.Services.Interfaces;
using PHPT.Common.DTOs;
using PHPT.Common.Enums;
using PHPT.Common.Models;
using System.Security.Claims;

namespace PHPT.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Patients")]
[Authorize(Roles = UserRoles.FrontDeskStaff)]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    /// <summary>
    /// Get all patients with pagination, filtering, and search
    /// </summary>
    [HttpGet("GetAllPatients")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PatientDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PatientDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<PatientDto>>>> GetPatients([FromQuery] PatientFilterDto filter)
    {
        try
        {
            var facilityId = GetFacilityId();
            var result = await _patientService.GetPatientsAsync(facilityId, filter);
            
            return Ok(ApiResponse<PagedResult<PatientDto>>.SuccessResponse(
                result, 
                $"Found {result.TotalCount} patient(s)"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patients");
            return StatusCode(500, ApiResponse<PagedResult<PatientDto>>.ErrorResponse(
                "Error fetching patients", 
                ex.Message));
        }
    }

    /// <summary>
    /// Get patient by ID
    /// </summary>
    //[HttpGet("{id}")]
    [HttpGet("GetPatientById/{patientId}")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PatientDto>>> GetPatientById(Guid patientId)
    {
        try
        {
            var facilityId = GetFacilityId();
            var patient = await _patientService.GetPatientByIdAsync(patientId, facilityId);
            
            if (patient == null)
            {
                return NotFound(ApiResponse<PatientDto>.ErrorResponse(
                    "Patient not found", 
                    "The requested patient does not exist or does not belong to your facility"));
            }

            return Ok(ApiResponse<PatientDto>.SuccessResponse(patient, "Patient retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patient {PatientId}", patientId);
            return StatusCode(500, ApiResponse<PatientDto>.ErrorResponse(
                "Error fetching patient", 
                ex.Message));
        }
    }

    /// <summary>
    /// Get patient by patient code
    /// </summary>
    [HttpGet("GetPatientByPatientCode/{patientCode}")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PatientDto>>> GetPatientByCode(string patientCode)
    {
        try
        {
            var facilityId = GetFacilityId();
            var patient = await _patientService.GetPatientByCodeAsync(patientCode, facilityId);
            
            if (patient == null)
            {
                return NotFound(ApiResponse<PatientDto>.ErrorResponse(
                    "Patient not found", 
                    $"No patient found with code: {patientCode}"));
            }

            return Ok(ApiResponse<PatientDto>.SuccessResponse(patient, "Patient retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patient by code {PatientCode}", patientCode);
            return StatusCode(500, ApiResponse<PatientDto>.ErrorResponse(
                "Error fetching patient", 
                ex.Message));
        }
    }

    /// <summary>
    /// Get patient by email address
    /// </summary>
    [HttpGet("GetPatientByEmail/{email}")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PatientDto>>> GetPatientByEmail(string email)
    {
        try
        {
            var facilityId = GetFacilityId();
            var patient = await _patientService.GetPatientByEmailAsync(email, facilityId);
            
            if (patient == null)
            {
                return NotFound(ApiResponse<PatientDto>.ErrorResponse(
                    "Patient not found", 
                    $"No patient found with email: {email}"));
            }

            return Ok(ApiResponse<PatientDto>.SuccessResponse(patient, "Patient retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patient by email {Email}", email);
            return StatusCode(500, ApiResponse<PatientDto>.ErrorResponse(
                "Error fetching patient", 
                ex.Message));
        }
    }

    /// <summary>
    /// Create a new patient
    /// </summary>
    [HttpPost("Add-Patient")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PatientDto>>> CreatePatient([FromBody] CreatePatientDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<PatientDto>.ErrorResponse("Validation failed", errors));
            }

            // Validate date of birth
            if (dto.DateOfBirth >= DateTime.Today)
            {
                return BadRequest(ApiResponse<PatientDto>.ErrorResponse(
                    "Invalid date of birth", 
                    "Date of birth must be in the past"));
            }

            var facilityId = GetFacilityId();
            var userId = GetCurrentUserId();
            
            var patient = await _patientService.CreatePatientAsync(facilityId, dto, userId);
            
            return CreatedAtAction(
                nameof(GetPatientById), 
                new { patientId = patient.Id },
                ApiResponse<PatientDto>.SuccessResponse(
                    patient, 
                    $"Patient created successfully with code: {patient.PatientCode}"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating patient");
            return BadRequest(ApiResponse<PatientDto>.ErrorResponse("Invalid operation", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            return StatusCode(500, ApiResponse<PatientDto>.ErrorResponse(
                "Error creating patient", 
                ex.Message));
        }
    }

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }
        return userId;
    }

    private Guid GetFacilityId()
    {
        var facilityIdClaim = User.FindFirst("FacilityId")?.Value;
        if (string.IsNullOrEmpty(facilityIdClaim) || !Guid.TryParse(facilityIdClaim, out var facilityId))
        {
            throw new UnauthorizedAccessException("Facility ID not found in claims");
        }
        return facilityId;
    }

    #endregion
}
