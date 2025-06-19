using Project.DTOs.RevenueDTOs;

namespace Project.Services.Interfaces;

public interface IRevenueService
{
    Task<RevenueResponseDto> CalculateRevenue(RevenueQueryDto query);
}