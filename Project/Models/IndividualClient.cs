using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public class IndividualClient : Client
{
    [MaxLength(50)]
    public string FirstName { get; set; } = null!;
    
    [MaxLength(50)]
    public string LastName { get; set; } = null!;
    
    [MaxLength(11)]
    public string PESEL { get; set; } = null!;
}