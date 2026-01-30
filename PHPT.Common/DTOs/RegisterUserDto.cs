using System.ComponentModel.DataAnnotations;

namespace PHPT.Common.DTOs;

public class RegisterUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public Guid FacilityId { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }
}
