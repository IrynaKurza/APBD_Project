using System.ComponentModel.DataAnnotations;

namespace Project.DTOs.SoftwareDTOs;

public class UpdateSoftwareDto
{
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    [MaxLength(20, ErrorMessage = "Version cannot exceed 20 characters")]
    public string? CurrentVersion { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Annual license cost must be greater than or equal to 0")]
    public decimal? AnnualLicenseCost { get; set; }
}