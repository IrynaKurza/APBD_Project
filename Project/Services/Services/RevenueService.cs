using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.DTOs.RevenueDTOs;
using Project.Services.Interfaces;

namespace Project.Services.Services;

public class RevenueService : IRevenueService
{
    private readonly DatabaseContext _context;

    public RevenueService(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<RevenueResponseDto> CalculateCurrentRevenue(RevenueQueryDto query)
    {
        var paymentsQuery = _context.Payments
            .Include(p => p.Contract)
            .Where(p => p.Contract.IsSigned);

        if (query.SoftwareId.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.Contract.SoftwareId == query.SoftwareId.Value);

        var totalRevenue = await paymentsQuery.SumAsync(p => p.Amount);

        return new RevenueResponseDto
        {
            Amount = totalRevenue,
            Currency = query.Currency,
            CalculationType = "Current"
        };
    }

    public async Task<RevenueResponseDto> CalculatePredictedRevenue(RevenueQueryDto query)
    {
        // Current revenue
        var current = await CalculateCurrentRevenue(query);

        // Add unsigned contracts
        var unsignedContractsQuery = _context.Contracts
            .Where(c => !c.IsSigned && !c.IsCancelled && c.EndDate > DateTime.UtcNow);

        if (query.SoftwareId.HasValue)
            unsignedContractsQuery = unsignedContractsQuery.Where(c => c.SoftwareId == query.SoftwareId.Value);

        var predictedAdditionalRevenue = await unsignedContractsQuery.SumAsync(c => c.Price);

        return new RevenueResponseDto
        {
            Amount = current.Amount + predictedAdditionalRevenue,
            Currency = query.Currency,
            CalculationType = "Predicted"
        };
    }
}