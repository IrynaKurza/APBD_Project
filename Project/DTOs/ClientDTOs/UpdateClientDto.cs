using System.ComponentModel.DataAnnotations;

namespace Project.DTOs.ClientDTOs;

public class UpdateClientDto
{
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string? Email { get; set; }
    
    [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string? Address { get; set; }
}