namespace Project.DTOs.PaymentDTOs;

public class PaymentResponseDto
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public bool ContractFullyPaid { get; set; }
    public decimal RemainingBalance { get; set; }
}