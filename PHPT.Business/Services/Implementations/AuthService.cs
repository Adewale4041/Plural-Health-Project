using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PHPT.Business.Services.Interfaces;
using PHPT.Common.DTOs;
using PHPT.Common.Enums;
using PHPT.Common.Models;
using PHPT.Data.Entities;
using PHPT.Data.UnitOfWork;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PHPT.Business.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User is deactivated {Email}", dto.Email);
                throw new UnauthorizedAccessException("Your account has been deactivated. Please contact your administrator.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Login failed: Invalid password for {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? UserRoles.FrontDeskStaff;

            var token = GenerateJwtToken(user, role);

            _logger.LogInformation("User logged in successfully: {Email}, Role: {Role}", dto.Email, role);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                FullName = user.FullName,
                Role = role,
                FacilityId = user.FacilityId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", dto.Email);
            throw;
        }
    }

    public async Task<AuthResponseDto> RegisterAdminAsync(RegisterUserDto dto)
    {
        try
        {
            // Check if facility exists
            var facility = await _unitOfWork.Facilities.GetByIdAsync(dto.FacilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Ensure Admin role exists
            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(UserRoles.Admin));
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                FacilityId = dto.FacilityId,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            await _userManager.AddToRoleAsync(user, UserRoles.Admin);

            _logger.LogInformation("Admin user created successfully: {Email}", dto.Email);

            var token = GenerateJwtToken(user, UserRoles.Admin);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                FullName = user.FullName,
                Role = UserRoles.Admin,
                FacilityId = user.FacilityId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin user {Email}", dto.Email);
            throw;
        }
    }

    public async Task<UserDto> RegisterFrontDeskStaffAsync(RegisterUserDto dto, Guid createdBy)
    {
        try
        {
            // Get the creating user
            var creator = await _userManager.FindByIdAsync(createdBy.ToString());
            if (creator == null)
            {
                throw new UnauthorizedAccessException("Creator user not found");
            }

            // Check if facility exists
            var facility = await _unitOfWork.Facilities.GetByIdAsync(dto.FacilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }

            // Ensure the facility matches the creator's facility
            if (creator.FacilityId != dto.FacilityId)
            {
                throw new UnauthorizedAccessException("Cannot create users for a different facility");
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Ensure FrontDeskStaff role exists
            if (!await _roleManager.RoleExistsAsync(UserRoles.FrontDeskStaff))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(UserRoles.FrontDeskStaff));
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                FacilityId = dto.FacilityId,
                IsActive = true,
                EmailConfirmed = true,
                CreatedBy = createdBy
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            await _userManager.AddToRoleAsync(user, UserRoles.FrontDeskStaff);

            _logger.LogInformation("FrontDeskStaff user created by {CreatorEmail}: {Email}", creator.Email, dto.Email);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                FacilityId = user.FacilityId,
                FacilityName = facility.Name,
                Role = UserRoles.FrontDeskStaff,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                CreatedByName = creator.FullName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating front desk staff user {Email}", dto.Email);
            throw;
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return null;
        }

        var facility = await _unitOfWork.Facilities.GetByIdAsync(user.FacilityId);
        var roles = await _userManager.GetRolesAsync(user);
        
        ApplicationUser? creator = null;
        if (user.CreatedBy.HasValue)
        {
            creator = await _userManager.FindByIdAsync(user.CreatedBy.Value.ToString());
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            FacilityId = user.FacilityId,
            FacilityName = facility?.Name ?? string.Empty,
            Role = roles.FirstOrDefault() ?? string.Empty,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            CreatedByName = creator?.FullName
        };
    }

    public async Task<List<UserDto>> GetFacilityStaffAsync(Guid facilityId, Guid requestingUserId)
    {
        var requestingUser = await _userManager.FindByIdAsync(requestingUserId.ToString());
        if (requestingUser == null || requestingUser.FacilityId != facilityId)
        {
            throw new UnauthorizedAccessException("Cannot access staff from a different facility");
        }

        var users = _userManager.Users
            .Where(u => u.FacilityId == facilityId)
            .ToList();

        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var facility = await _unitOfWork.Facilities.GetByIdAsync(user.FacilityId);
            var roles = await _userManager.GetRolesAsync(user);
            
            ApplicationUser? creator = null;
            if (user.CreatedBy.HasValue)
            {
                creator = await _userManager.FindByIdAsync(user.CreatedBy.Value.ToString());
            }

            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                FacilityId = user.FacilityId,
                FacilityName = facility?.Name ?? string.Empty,
                Role = roles.FirstOrDefault() ?? string.Empty,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                CreatedByName = creator?.FullName
            });
        }

        return userDtos.OrderBy(u => u.CreatedAt).ToList();
    }

    public async Task<bool> DeactivateUserAsync(Guid userId, Guid requestingUserId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var requestingUser = await _userManager.FindByIdAsync(requestingUserId.ToString());
        if (requestingUser == null || requestingUser.FacilityId != user.FacilityId)
        {
            throw new UnauthorizedAccessException("Cannot deactivate users from a different facility");
        }

        if (userId == requestingUserId)
        {
            throw new InvalidOperationException("Cannot deactivate your own account");
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        
        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} deactivated by {RequestingEmail}", user.Email, requestingUser.Email);
        }

        return result.Succeeded;
    }

    public async Task<bool> ActivateUserAsync(Guid userId, Guid requestingUserId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var requestingUser = await _userManager.FindByIdAsync(requestingUserId.ToString());
        if (requestingUser == null || requestingUser.FacilityId != user.FacilityId)
        {
            throw new UnauthorizedAccessException("Cannot activate users from a different facility");
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        
        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} activated by {RequestingEmail}", user.Email, requestingUser.Email);
        }

        return result.Succeeded;
    }

    private string GenerateJwtToken(ApplicationUser user, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, role),
            new Claim("FacilityId", user.FacilityId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
