namespace PHPT.Common.DTOs;

public class ClinicFilterDto
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool SortByNameAscending { get; set; } = true;
}
