using System.ComponentModel.DataAnnotations;

namespace Project.DTOs.ClientDTOs;

public class CreateCompanyClientDto
{
    [Required(ErrorMessage = "Company name is required")]
    [MaxLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
    public string CompanyName { get; set; } = null!;
    
    [Required(ErrorMessage = "KRS number is required")]
    [MaxLength(10, ErrorMessage = "KRS number must be exactly 10 digits")]
    [MinLength(10, ErrorMessage = "KRS number must be exactly 10 digits")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "KRS number must contain exactly 10 digits")]
    public string KRSNumber { get; set; } = null!;
    
    [Required(ErrorMessage = "Address is required")]
    [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string Address { get; set; } = null!;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = null!;
    
    [Required(ErrorMessage = "Phone number is required")]
    [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
    public string PhoneNumber { get; set; } = null!;
}