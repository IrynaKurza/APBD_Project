using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models;

[Table("Employee")]
public class Employee
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(50)]
    public string FirstName { get; set; } = null!;
    
    [MaxLength(50)]
    public string LastName { get; set; } = null!;
    
    [MaxLength(100)]
    public string Email { get; set; } = null!;
    
    [MaxLength(256)]
    public string PasswordHash { get; set; } = null!;
    
    public int RoleId { get; set; }
    
    [ForeignKey(nameof(RoleId))]
    public virtual EmployeeRole Role { get; set; } = null!;
    
    public virtual RefreshToken RefreshToken { get; set; } = null!;
}