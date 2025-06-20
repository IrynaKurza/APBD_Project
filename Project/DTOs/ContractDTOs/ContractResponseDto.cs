namespace Project.DTOs.ContractDTOs;

public class ContractResponseDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = null!;
    public string SoftwareName { get; set; } = null!;
    public string SoftwareVersion { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public bool IsSigned { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingAmount { get; set; }
}