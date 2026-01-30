using System.ComponentModel.DataAnnotations;

namespace PHPT.Common.DTOs;

public class UpdateClinicDto
{
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
