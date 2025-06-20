namespace Project.DTOs.ContractDTOs;

public class CreateContractDto
{
    public int ClientId { get; set; }
    public int SoftwareId { get; set; }
    public string SoftwareVersion { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int AdditionalSupportYears { get; set; }
}