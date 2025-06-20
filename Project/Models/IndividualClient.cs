using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public class IndividualClient : Client
{
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;
    
    [MaxLength(11)]
    public string PESEL { get; set; } = string.Empty;
}