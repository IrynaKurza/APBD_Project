using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models;

[Table("Employee")]
public class Employee
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(50)]
    public string FirstName { get; set; }
    
    [MaxLength(50)]
    public string LastName { get; set; }
    
    [MaxLength(100)]
    public string Email { get; set; }
    
    [MaxLength(256)]
    public string PasswordHash { get; set; }
    
    public int RoleId { get; set; }
    
    [ForeignKey(nameof(RoleId))]
    public virtual EmployeeRole Role { get; set; }
    
    public virtual RefreshToken RefreshToken { get; set; }
}