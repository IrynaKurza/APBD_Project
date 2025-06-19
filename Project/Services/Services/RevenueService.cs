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
        // 1. Calculate CURRENT revenue - from SIGNED contracts only  
        var signedContractsQuery = _context.Contracts
            .Where(c => c.IsSigned && !c.IsCancelled);

        if (query.SoftwareId.HasValue)
            signedContractsQuery = signedContractsQuery.Where(c => c.SoftwareId == query.SoftwareId.Value);

        var currentRevenue = await signedContractsQuery.SumAsync(c => c.Price);

        // 2. Calculate PREDICTED revenue - add unsigned contracts
        var unsignedContractsQuery = _context.Contracts
            .Where(c => !c.IsSigned && !c.IsCancelled && c.EndDate > DateTime.UtcNow);

        if (query.SoftwareId.HasValue)
            unsignedContractsQuery = unsignedContractsQuery.Where(c => c.SoftwareId == query.SoftwareId.Value);

        var predictedAdditionalRevenue = await unsignedContractsQuery.SumAsync(c => c.Price);
        var totalRevenue = currentRevenue + predictedAdditionalRevenue; 

        // 3. Currency conversion
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
            CalculationType = "Predicted" 
        };
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