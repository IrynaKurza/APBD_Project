using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace Project.Models;

public abstract class Client
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(200)]
    public string Address { get; set; }
    
    [MaxLength(100)]
    public string Email { get; set; }
    
    [MaxLength(20)]
    public string PhoneNumber { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    public ICollection<Contract> Contracts { get; set; }
}