namespace PHPT.Common.DTOs;

public class PatientFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public string? Gender { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool SortByNameAscending { get; set; } = true;
}
