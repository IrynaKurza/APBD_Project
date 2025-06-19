using System.ComponentModel.DataAnnotations;

namespace Project.DTOs.PaymentDTOs;

public class CreatePaymentDto
{
    [Required(ErrorMessage = "Contract ID is required")]
    public int ContractId { get; set; }
    
    [Required(ErrorMessage = "Payment amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required(ErrorMessage = "Payment method is required")]
    [MaxLength(50, ErrorMessage = "Payment method cannot exceed 50 characters")]
    public string PaymentMethod { get; set; } = null!;
}