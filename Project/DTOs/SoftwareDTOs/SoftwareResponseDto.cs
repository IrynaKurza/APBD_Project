namespace Project.DTOs.SoftwareDTOs;

public class SoftwareResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string CurrentVersion { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal AnnualLicenseCost { get; set; }
    public int ActiveContractsCount { get; set; }
    public decimal TotalRevenue { get; set; }
}