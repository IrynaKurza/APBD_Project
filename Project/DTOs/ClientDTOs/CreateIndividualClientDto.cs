using System.ComponentModel.DataAnnotations;

namespace Project.DTOs.ClientDTOs;

public class CreateIndividualClientDto
{
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = null!;
    
    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = null!;
    
    [Required(ErrorMessage = "PESEL is required")]
    [MaxLength(11, ErrorMessage = "PESEL must be exactly 11 digits")]
    [MinLength(11, ErrorMessage = "PESEL must be exactly 11 digits")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL must contain exactly 11 digits")]
    public string PESEL { get; set; } = null!;
    
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