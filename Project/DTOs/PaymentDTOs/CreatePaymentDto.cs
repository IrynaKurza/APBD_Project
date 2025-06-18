namespace Project.DTOs.PaymentDTOs;

public class CreatePaymentDto
{
    public int ContractId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
}