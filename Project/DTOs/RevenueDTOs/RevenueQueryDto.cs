using System.ComponentModel.DataAnnotations;

namespace Project.DTOs.RevenueDTOs;

public class RevenueQueryDto
{
    public int? SoftwareId { get; set; }
    public string Currency { get; set; } = "PLN";
    public string RevenueType { get; set; } = "Current";
}