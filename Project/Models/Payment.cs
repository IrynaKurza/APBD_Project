using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project.Models;

[Table("Payment")]
public class Payment
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(Contract))]
    public int ContractId { get; set; }
    
    [Column(TypeName = "decimal")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }
    
    public DateTime PaymentDate { get; set; }
    
    [MaxLength(50)]
    public string PaymentMethod { get; set; }
    
    public Contract Contract { get; set; }
}