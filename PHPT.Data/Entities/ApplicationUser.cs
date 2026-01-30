using Microsoft.AspNetCore.Identity;

namespace PHPT.Data.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid FacilityId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    // Navigation properties
    public Facility Facility { get; set; } = null!;
    public ApplicationUser? Creator { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
