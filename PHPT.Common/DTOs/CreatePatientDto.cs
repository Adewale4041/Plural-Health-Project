using System.ComponentModel.DataAnnotations;

namespace PHPT.Common.DTOs;

public class CreatePatientDto
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [StringLength(10)]
    public string Gender { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal InitialWalletBalance { get; set; } = 0;
}
