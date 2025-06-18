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
    public string Name { get; set; }
    
    [MaxLength(500)]
    public string Description { get; set; }
    
    [MaxLength(20)]
    public string CurrentVersion { get; set; }
    
    [MaxLength(50)]
    public string Category { get; set; }
    
    [Column(TypeName = "decimal")]
    [Precision(18, 2)]
    public decimal AnnualLicenseCost { get; set; }
    
    public ICollection<Contract> Contracts { get; set; }
    public ICollection<Discount> Discounts { get; set; }
}