using PHPT.Common.DTOs;

namespace PHPT.Business.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RegisterAdminAsync(RegisterUserDto dto);
    Task<UserDto> RegisterFrontDeskStaffAsync(RegisterUserDto dto, Guid createdBy);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<List<UserDto>> GetFacilityStaffAsync(Guid facilityId, Guid requestingUserId);
    Task<bool> DeactivateUserAsync(Guid userId, Guid requestingUserId);
    Task<bool> ActivateUserAsync(Guid userId, Guid requestingUserId);
}
