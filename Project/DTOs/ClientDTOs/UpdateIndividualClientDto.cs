using System.ComponentModel.DataAnnotations;

namespace Project.DTOs.ClientDTOs;

public class UpdateIndividualClientDto : UpdateClientDto
{
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName { get; set; }
    
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? LastName { get; set; }
}