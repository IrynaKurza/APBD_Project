using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Discount")]
public class Discount
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string Name { get; set; }
    
    [Column(TypeName = "decimal")]
    [Precision(5, 2)]
    public decimal Percentage { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsForContracts { get; set; }
    
    [ForeignKey(nameof(Software))]
    public int? SoftwareId { get; set; }
    
    public Software Software { get; set; }
}