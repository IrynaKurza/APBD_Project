using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Contract")]
public class Contract
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(Client))]
    public int ClientId { get; set; }
    
    [ForeignKey(nameof(Software))]
    public int SoftwareId { get; set; }
    
    [MaxLength(20)]
    public string SoftwareVersion { get; set; } = null!;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    [Column(TypeName = "decimal")]
    [Precision(18, 2)]
    public decimal Price { get; set; }
    
    public int AdditionalSupportYears { get; set; }
    public bool IsSigned { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Client Client { get; set; } = null!;
    public Software Software { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}