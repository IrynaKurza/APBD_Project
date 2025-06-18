using System.ComponentModel.DataAnnotations;

namespace Project.Models;

public class IndividualClient : Client
{
    [MaxLength(50)]
    public string FirstName { get; set; }
    
    [MaxLength(50)]
    public string LastName { get; set; }
    
    [MaxLength(11)]
    public string PESEL { get; set; }
}