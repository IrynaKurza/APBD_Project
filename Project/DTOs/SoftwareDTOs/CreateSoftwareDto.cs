using System.ComponentModel.DataAnnotations;

namespace Project.DTOs.SoftwareDTOs;

public class CreateSoftwareDto
{
    [Required(ErrorMessage = "Software name is required")]
    [MaxLength(100, ErrorMessage = "Software name cannot exceed 100 characters")]
    public string Name { get; set; } = null!;
    
    [Required(ErrorMessage = "Description is required")]
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = null!;
    
    [Required(ErrorMessage = "Current version is required")]
    [MaxLength(20, ErrorMessage = "Version cannot exceed 20 characters")]
    public string CurrentVersion { get; set; } = null!;
    
    [Required(ErrorMessage = "Category is required")]
    [MaxLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
    public string Category { get; set; } = null!;
    
    [Required(ErrorMessage = "Annual license cost is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Annual license cost must be greater than or equal to 0")]
    public decimal AnnualLicenseCost { get; set; }
}