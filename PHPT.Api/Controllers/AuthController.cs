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
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }



    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse("Login failed", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, ApiResponse<AuthResponseDto>.ErrorResponse("Login error", ex.Message));
        }
    }




    [HttpPost("register-admin")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RegisterAdmin([FromBody] RegisterUserDto dto)
    {
        try
        {
            var result = await _authService.RegisterAdminAsync(dto);
            return CreatedAtAction(nameof(GetCurrentUser), null, 
                ApiResponse<AuthResponseDto>.SuccessResponse(result, "Admin registered successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering admin");
            return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse("Registration failed", ex.Message));
        }
    }




    [HttpPost("register-staff")]
    [Authorize(Roles = UserRoles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<UserDto>>> RegisterStaff([FromBody] RegisterUserDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _authService.RegisterFrontDeskStaffAsync(dto, userId);
            return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, 
                ApiResponse<UserDto>.SuccessResponse(result, "Staff registered successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<UserDto>.ErrorResponse("Forbidden", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering staff");
            return BadRequest(ApiResponse<UserDto>.ErrorResponse("Registration failed", ex.Message));
        }
    }




    [HttpGet("Me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _authService.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found", "User not found"));
            }

            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("Error", ex.Message));
        }
    }




    [HttpGet("users/{id}")]
    [Authorize(Roles = UserRoles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(Guid id)
    {
        try
        {
            var user = await _authService.GetUserByIdAsync(id);
            
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found", "User not found"));
            }

            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("Error", ex.Message));
        }
    }




    [HttpGet("staff")]
    [Authorize(Roles = UserRoles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetFacilityStaff()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var facilityId = Guid.Parse(User.FindFirst("FacilityId")!.Value);
            
            var staff = await _authService.GetFacilityStaffAsync(facilityId, userId);
            return Ok(ApiResponse<List<UserDto>>.SuccessResponse(staff, "Staff retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting facility staff");
            return StatusCode(500, ApiResponse<List<UserDto>>.ErrorResponse("Error", ex.Message));
        }
    }



    [HttpPut("users/{id}/deactivate")]
    [Authorize(Roles = UserRoles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<string>>> DeactivateUser(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _authService.DeactivateUserAsync(id, userId);
            
            if (result)
            {
                return Ok(ApiResponse<string>.SuccessResponse("Success", "User deactivated successfully"));
            }

            return BadRequest(ApiResponse<string>.ErrorResponse("Failed", "Failed to deactivate user"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse("Invalid operation", ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<string>.ErrorResponse("Forbidden", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", id);
            return BadRequest(ApiResponse<string>.ErrorResponse("Error", ex.Message));
        }
    }



    [HttpPut("users/{id}/activate")]
    [Authorize(Roles = UserRoles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<string>>> ActivateUser(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _authService.ActivateUserAsync(id, userId);
            
            if (result)
            {
                return Ok(ApiResponse<string>.SuccessResponse("Success", "User activated successfully"));
            }

            return BadRequest(ApiResponse<string>.ErrorResponse("Failed", "Failed to activate user"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse("Invalid operation", ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<string>.ErrorResponse("Forbidden", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user {UserId}", id);
            return BadRequest(ApiResponse<string>.ErrorResponse("Error", ex.Message));
        }
    }
}
