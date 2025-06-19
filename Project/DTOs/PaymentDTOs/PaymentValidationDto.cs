namespace Project.DTOs.PaymentDTOs;

public class PaymentValidationDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal RemainingBalance { get; set; }
    public bool WillCompleteContract { get; set; }
}