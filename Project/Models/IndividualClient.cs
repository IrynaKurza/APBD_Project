using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public class IndividualClient : Client
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(11)]
    public string PESEL { get; set; } = string.Empty;
}