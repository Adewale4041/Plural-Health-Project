namespace PHPT.Common.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid FacilityId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
