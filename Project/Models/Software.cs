using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Software")]
public class Software
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string CurrentVersion { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal")]
    [Precision(18, 2)]
    public decimal AnnualLicenseCost { get; set; }
    
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
}