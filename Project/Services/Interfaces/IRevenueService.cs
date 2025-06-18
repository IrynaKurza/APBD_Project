using Project.DTOs.RevenueDTOs;

namespace Project.Services.Interfaces;

public interface IRevenueService
{
    Task<RevenueResponseDto> CalculateCurrentRevenue(RevenueQueryDto query);
    Task<RevenueResponseDto> CalculatePredictedRevenue(RevenueQueryDto query);
}