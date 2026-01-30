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
[Tags("Appointments")]
[Authorize(Roles = UserRoles.FrontDeskStaff)]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(IAppointmentService appointmentService, ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    /// <summary>
    /// Get a paginated list of appointments with filtering and search
    /// </summary>
    [HttpGet("GetAllAppointments")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AppointmentListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AppointmentListDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<AppointmentListDto>>>> GetAppointments(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? clinicId = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool sortByTimeAscending = true)
    {
        try
        {
            var actualFacilityId = GetFacilityId();

            var filterDto = new AppointmentFilterDto
            {
                StartDate = startDate,
                EndDate = endDate,
                ClinicId = clinicId,
                SearchTerm = searchTerm,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortByTimeAscending = sortByTimeAscending
            };

            var result = await _appointmentService.GetAppointmentsAsync(actualFacilityId, filterDto);
            return Ok(ApiResponse<PagedResult<AppointmentListDto>>.SuccessResponse(result, "Appointments loaded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading appointments");
            return StatusCode(500, ApiResponse<PagedResult<AppointmentListDto>>.ErrorResponse("Error loading appointments", ex.Message));
        }
    }

    /// <summary>
    /// Get detailed information about a specific appointment
    /// </summary>
    [HttpGet("GetAppointmentById/{Id}")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDetailsDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDetailsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AppointmentDetailsDto>>> GetAppointment(Guid Id)
    {
        try
        {
            var actualFacilityId = GetFacilityId();
            var result = await _appointmentService.GetAppointmentByIdAsync(Id, actualFacilityId);
            
            if (result == null)
            {
                return NotFound(ApiResponse<AppointmentDetailsDto>.ErrorResponse("Appointment not found", "The requested appointment does not exist"));
            }

            return Ok(ApiResponse<AppointmentDetailsDto>.SuccessResponse(result, "Appointment loaded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading appointment {AppointmentId}", Id);
            return StatusCode(500, ApiResponse<AppointmentDetailsDto>.ErrorResponse("Error loading appointment", ex.Message));
        }
    }

    /// <summary>
    /// Create a new appointment
    /// </summary>
    [HttpPost("ScheduleAppointment")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDetailsDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDetailsDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDetailsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AppointmentDetailsDto>>> CreateAppointment([FromBody] CreateAppointmentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AppointmentDetailsDto>.ErrorResponse("Validation failed", errors));
            }

            var actualFacilityId = GetFacilityId();
            var result = await _appointmentService.CreateAppointmentAsync(actualFacilityId, dto);
            return CreatedAtAction(nameof(GetAppointment), new { id = result.Id, facilityId = actualFacilityId }, 
                ApiResponse<AppointmentDetailsDto>.SuccessResponse(result, "Appointment created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating appointment");
            return BadRequest(ApiResponse<AppointmentDetailsDto>.ErrorResponse("Invalid operation", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            return StatusCode(500, ApiResponse<AppointmentDetailsDto>.ErrorResponse("Error creating appointment", ex.Message));
        }
    }

    /// <summary>
    /// Update appointment status to AwaitingVitals
    /// </summary>
    [HttpPatch("UpdateAppointmentStatus/{Id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<string>>> UpdateAppointmentStatus(Guid Id)
    {
        try
        {
            var actualFacilityId = GetFacilityId();
            await _appointmentService.UpdateAppointmentStatusAsync(Id, actualFacilityId);
            return Ok(ApiResponse<string>.SuccessResponse("Status updated", "Appointment status updated to AwaitingVitals"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating appointment status");
            return BadRequest(ApiResponse<string>.ErrorResponse("Invalid operation", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment status");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Error updating appointment status", ex.Message));
        }
    }


    #region Helper Methods

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
