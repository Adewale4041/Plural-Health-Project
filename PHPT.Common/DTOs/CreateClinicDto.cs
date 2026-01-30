using System.ComponentModel.DataAnnotations;

namespace PHPT.Common.DTOs;

public class CreateClinicDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
