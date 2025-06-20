using System.ComponentModel.DataAnnotations;

namespace Project.DTOs.ClientDTOs;

public class UpdateCompanyClientDto : UpdateClientDto
{
    [MaxLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
    public string? CompanyName { get; set; }
}