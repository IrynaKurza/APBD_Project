using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models;

[Table("RefreshToken")]
public class RefreshToken
{
    [Key]
    public int EmployeeId { get; set; }
    
    [MaxLength(128)]
    public string Token { get; set; }
    
    public DateTime ExpiresAt { get; set; }
    
    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee Employee { get; set; }
}