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
    
    public async Task<RevenueResponseDto> CalculateRevenue(RevenueQueryDto query)
    {
        decimal totalRevenue;
        string calculationType;

        // Determine which type of revenue to calculate
        if (query.RevenueType.ToLower() == "predicted")
        {
            totalRevenue = await CalculatePredictedRevenue(query.SoftwareId);
            calculationType = "Predicted";
        }
        else
        {
            totalRevenue = await CalculateCurrentRevenue(query.SoftwareId);
            calculationType = "Current";
        }

        // Currency conversion
        var currency = query.Currency;
        if (currency != "PLN")
        {
            var exchangeRate = GetSimpleExchangeRate(currency);
            totalRevenue *= exchangeRate;
        }

        return new RevenueResponseDto
        {
            Amount = Math.Round(totalRevenue, 2),
            Currency = currency,
            CalculationType = calculationType
        };
    }
    
    
    // Calculate CURRENT revenue - only from SIGNED contracts
    private async Task<decimal> CalculateCurrentRevenue(int? softwareId)
    {
        var signedContractsQuery = _context.Contracts
            .Where(c => c.IsSigned && !c.IsCancelled);

        if (softwareId.HasValue)
            signedContractsQuery = signedContractsQuery.Where(c => c.SoftwareId == softwareId.Value);

        return await signedContractsQuery.SumAsync(c => c.Price);
    }


    // Calculate PREDICTED revenue - signed contracts + unsigned active contracts
    // Business assumption: "All unsigned contracts will eventually be signed"
    private async Task<decimal> CalculatePredictedRevenue(int? softwareId)
    {
        // 1. Get current revenue (signed contracts)
        var currentRevenue = await CalculateCurrentRevenue(softwareId);

        // 2. Get potential revenue from unsigned contracts (still within deadline)
        var unsignedContractsQuery = _context.Contracts
            .Where(c => !c.IsSigned && 
                       !c.IsCancelled && 
                       c.EndDate > DateTime.UtcNow); // Only active contracts

        if (softwareId.HasValue)
            unsignedContractsQuery = unsignedContractsQuery.Where(c => c.SoftwareId == softwareId.Value);

        var potentialRevenue = await unsignedContractsQuery.SumAsync(c => c.Price);

        // 3. Predicted = Current + Potential
        return currentRevenue + potentialRevenue;
    }

    private decimal GetSimpleExchangeRate(string currency)
    {
        return currency.ToUpper() switch
        {
            "USD" => 0.25m,
            "EUR" => 0.23m,
            "GBP" => 0.20m,
            _ => 1.0m
        };
    }
}