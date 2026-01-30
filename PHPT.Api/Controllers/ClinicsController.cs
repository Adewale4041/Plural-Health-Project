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
[Tags("Clinics")]
[Authorize(Roles = UserRoles.Admin)]
public class ClinicsController : ControllerBase
{
    private readonly IClinicService _clinicService;
    private readonly ILogger<ClinicsController> _logger;

    public ClinicsController(IClinicService clinicService, ILogger<ClinicsController> logger)
    {
        _clinicService = clinicService;
        _logger = logger;
    }

    /// <summary>
    /// Get all clinics with pagination and search (Admin only)
    /// </summary>
    [HttpGet("GetAllClinics")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ClinicDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ClinicDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<ClinicDto>>>> GetClinics([FromQuery] ClinicFilterDto filter)
    {
        try
        {
            var facilityId = GetFacilityId();
            var result = await _clinicService.GetClinicsAsync(facilityId, filter);
            
            return Ok(ApiResponse<PagedResult<ClinicDto>>.SuccessResponse(
                result, 
                $"Found {result.TotalCount} clinic(s)"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching clinics");
            return StatusCode(500, ApiResponse<PagedResult<ClinicDto>>.ErrorResponse(
                "Error fetching clinics", 
                ex.Message));
        }
    }

    /// <summary>
    /// Get clinic by ID (Admin only)
    /// </summary>
    [HttpGet("GetClinicById{Id}")]
    [ProducesResponseType(typeof(ApiResponse<ClinicDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClinicDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ClinicDto>>> GetClinicById(Guid Id)
    {
        try
        {
            var facilityId = GetFacilityId();
            var clinic = await _clinicService.GetClinicByIdAsync(Id, facilityId);
            
            if (clinic == null)
            {
                return NotFound(ApiResponse<ClinicDto>.ErrorResponse(
                    "Clinic not found", 
                    "The requested clinic does not exist or does not belong to your facility"));
            }

            return Ok(ApiResponse<ClinicDto>.SuccessResponse(clinic, "Clinic retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching clinic {ClinicId}", Id);
            return StatusCode(500, ApiResponse<ClinicDto>.ErrorResponse(
                "Error fetching clinic", 
                ex.Message));
        }
    }

    /// <summary>
    /// Get clinic by clinic code (Admin only)
    /// </summary>
    [HttpGet("GetClinicByCode/{code}")]
    [ProducesResponseType(typeof(ApiResponse<ClinicDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClinicDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ClinicDto>>> GetClinicByCode(string code)
    {
        try
        {
            var facilityId = GetFacilityId();
            var clinic = await _clinicService.GetClinicByCodeAsync(code, facilityId);
            
            if (clinic == null)
            {
                return NotFound(ApiResponse<ClinicDto>.ErrorResponse(
                    "Clinic not found", 
                    $"No clinic found with code: {code}"));
            }

            return Ok(ApiResponse<ClinicDto>.SuccessResponse(clinic, "Clinic retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching clinic by code {ClinicCode}", code);
            return StatusCode(500, ApiResponse<ClinicDto>.ErrorResponse(
                "Error fetching clinic", 
                ex.Message));
        }
    }

    /// <summary>
    /// Get clinic by clinic name (Admin only)
    /// </summary>
    [HttpGet("/GetClinicByName{name}")]
    [ProducesResponseType(typeof(ApiResponse<ClinicDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClinicDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ClinicDto>>> GetClinicByName(string name)
    {
        try
        {
            var facilityId = GetFacilityId();
            var clinic = await _clinicService.GetClinicByNameAsync(name, facilityId);
            
            if (clinic == null)
            {
                return NotFound(ApiResponse<ClinicDto>.ErrorResponse(
                    "Clinic not found", 
                    $"No clinic found with name: {name}"));
            }

            return Ok(ApiResponse<ClinicDto>.SuccessResponse(clinic, "Clinic retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching clinic by name {ClinicName}", name);
            return StatusCode(500, ApiResponse<ClinicDto>.ErrorResponse(
                "Error fetching clinic", 
                ex.Message));
        }
    }

    /// <summary>
    /// Create a new clinic (Admin only)
    /// </summary>
    [HttpPost("Add-Clinic")]
    [ProducesResponseType(typeof(ApiResponse<ClinicDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ClinicDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ClinicDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ClinicDto>>> CreateClinic([FromBody] CreateClinicDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<ClinicDto>.ErrorResponse("Validation failed", errors));
            }

            var facilityId = GetFacilityId();
            var userId = GetCurrentUserId();
            
            var clinic = await _clinicService.CreateClinicAsync(facilityId, dto, userId);
            
            return CreatedAtAction(
                nameof(GetClinicById), 
                new { id = clinic.Id }, 
                ApiResponse<ClinicDto>.SuccessResponse(
                    clinic, 
                    $"Clinic '{clinic.Name}' created successfully with code: {clinic.Code}"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating clinic");
            return BadRequest(ApiResponse<ClinicDto>.ErrorResponse("Invalid operation", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating clinic");
            return StatusCode(500, ApiResponse<ClinicDto>.ErrorResponse(
                "Error creating clinic", 
                ex.Message));
        }
    }

    /// <summary>
    /// Update an existing clinic (Admin only)
    /// </summary>
    [HttpPatch("UpdateClinic/{id}")]
    [ProducesResponseType(typeof(ApiResponse<ClinicDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClinicDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ClinicDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ClinicDto>>> UpdateClinic(Guid id, [FromBody] UpdateClinicDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<ClinicDto>.ErrorResponse("Validation failed", errors));
            }

            var facilityId = GetFacilityId();
            var userId = GetCurrentUserId();
            
            var clinic = await _clinicService.UpdateClinicAsync(id, facilityId, dto, userId);
            
            return Ok(ApiResponse<ClinicDto>.SuccessResponse(
                clinic, 
                "Clinic updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating clinic {ClinicId}", id);
            return BadRequest(ApiResponse<ClinicDto>.ErrorResponse("Invalid operation", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating clinic {ClinicId}", id);
            return StatusCode(500, ApiResponse<ClinicDto>.ErrorResponse(
                "Error updating clinic", 
                ex.Message));
        }
    }

    /// <summary>
    /// Deactivate a clinic (Admin only)
    /// </summary>
    [HttpPut("DeactivateClinic/{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<string>>> DeactivateClinic(Guid id)
    {
        try
        {
            var facilityId = GetFacilityId();
            var result = await _clinicService.DeactivateClinicAsync(id, facilityId);
            
            if (result)
            {
                return Ok(ApiResponse<string>.SuccessResponse(
                    "Success", 
                    "Clinic deactivated successfully"));
            }

            return BadRequest(ApiResponse<string>.ErrorResponse(
                "Failed", 
                "Failed to deactivate clinic"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while deactivating clinic {ClinicId}", id);
            return BadRequest(ApiResponse<string>.ErrorResponse("Invalid operation", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating clinic {ClinicId}", id);
            return StatusCode(500, ApiResponse<string>.ErrorResponse(
                "Error deactivating clinic", 
                ex.Message));
        }
    }

    /// <summary>
    /// Activate a clinic (Admin only)
    /// </summary>
    [HttpPut("Activate/{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<string>>> ActivateClinic(Guid id)
    {
        try
        {
            var facilityId = GetFacilityId();
            var result = await _clinicService.ActivateClinicAsync(id, facilityId);
            
            if (result)
            {
                return Ok(ApiResponse<string>.SuccessResponse(
                    "Success", 
                    "Clinic activated successfully"));
            }

            return BadRequest(ApiResponse<string>.ErrorResponse(
                "Failed", 
                "Failed to activate clinic"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while activating clinic {ClinicId}", id);
            return BadRequest(ApiResponse<string>.ErrorResponse("Invalid operation", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating clinic {ClinicId}", id);
            return StatusCode(500, ApiResponse<string>.ErrorResponse(
                "Error activating clinic", 
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
