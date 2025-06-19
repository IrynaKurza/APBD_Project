namespace Project.DTOs.RevenueDTOs;

public class RevenueResponseDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PLN";
    public string CalculationType { get; set; } = "Both";
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}