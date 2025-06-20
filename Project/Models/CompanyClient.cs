using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public class CompanyClient : Client
{
    [Required]
    [MaxLength(100)]
    public string CompanyName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(10)]
    public string KRSNumber { get; set; } = string.Empty;
}