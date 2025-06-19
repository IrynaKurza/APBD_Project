using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.DTOs.ContractDTOs;
using Project.Models;
using Project.Services.Interfaces;

namespace Project.Services.Services;

public class ContractService : IContractService
{
    private readonly DatabaseContext _context;
    private readonly IClientService _clientService;

    public ContractService(DatabaseContext context, IClientService clientService)
    {
        _context = context;
        _clientService = clientService;
    }

    public async Task<List<ContractResponseDto>> GetContracts()
    {
        var contracts = await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.Software)
            .Include(c => c.Payments)
            .Select(c => new ContractResponseDto
            {
                Id = c.Id,
                ClientName = c.Client is IndividualClient 
                    ? ((IndividualClient)c.Client).FirstName + " " + ((IndividualClient)c.Client).LastName
                    : ((CompanyClient)c.Client).CompanyName,
                SoftwareName = c.Software.Name,
                SoftwareVersion = c.SoftwareVersion,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Price = c.Price,
                IsSigned = c.IsSigned,
                TotalPaid = c.Payments.Sum(p => p.Amount),
                RemainingAmount = c.Price - c.Payments.Sum(p => p.Amount)
            }).ToListAsync();

        return contracts;
    }

    public async Task<ContractResponseDto?> GetContract(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.Software)
            .Include(c => c.Payments)
            .Where(c => c.Id == id)
            .Select(c => new ContractResponseDto
            {
                Id = c.Id,
                ClientName = c.Client is IndividualClient 
                    ? ((IndividualClient)c.Client).FirstName + " " + ((IndividualClient)c.Client).LastName
                    : ((CompanyClient)c.Client).CompanyName,
                SoftwareName = c.Software.Name,
                SoftwareVersion = c.SoftwareVersion,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Price = c.Price,
                IsSigned = c.IsSigned,
                TotalPaid = c.Payments.Sum(p => p.Amount),
                RemainingAmount = c.Price - c.Payments.Sum(p => p.Amount)
            }).FirstOrDefaultAsync();

        return contract;
    }

    public async Task<ContractResponseDto?> CreateContract(CreateContractDto dto)
    {
        // 1. Validate contract timeframe (3-30 days)
        var timespan = dto.EndDate - dto.StartDate;
        if (timespan.Days < 3 || timespan.Days > 30)
        {
            throw new ArgumentException("Contract payment period must be between 3 and 30 days");
        }

        // 2. Validate start date is not in the past
        if (dto.StartDate < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Contract start date cannot be in the past");
        }

        // 3. Validate additional support years (0-3 years only)
        if (dto.AdditionalSupportYears < 0 || dto.AdditionalSupportYears > 3)
        {
            throw new ArgumentException("Additional support can only be 0-3 years");
        }

        // 4. Check if software exists
        var software = await _context.Software.FindAsync(dto.SoftwareId);
        if (software == null) 
            throw new ArgumentException("Software not found");

        // 5. Check if client exists
        var client = await _context.Set<Client>().FindAsync(dto.ClientId);
        if (client == null) 
            throw new ArgumentException("Client not found");

        if (client.IsDeleted)
            throw new ArgumentException("Cannot create contract for deleted client");

        // 6. Check for active contracts (no duplicates)
        var hasActiveContract = await _context.Contracts
            .AnyAsync(c => c.ClientId == dto.ClientId && 
                          c.SoftwareId == dto.SoftwareId && 
                          !c.IsCancelled && 
                          (!c.IsSigned || c.EndDate > DateTime.UtcNow));

        if (hasActiveContract) 
            throw new InvalidOperationException("Client already has active contract for this software");

        // 7. Calculate price with proper discount logic
        var basePrice = software.AnnualLicenseCost + (dto.AdditionalSupportYears * 1000m);
        
        // Check if client is returning customer
        var isReturningClient = await _clientService.IsReturningClient(dto.ClientId);
        
        // Find best active discount for this software
        var activeDiscounts = await _context.Discounts
            .Where(d => d.StartDate <= DateTime.UtcNow && 
                       d.EndDate >= DateTime.UtcNow &&
                       d.IsForContracts &&
                       (d.SoftwareId == null || d.SoftwareId == dto.SoftwareId))
            .ToListAsync();

        var bestDiscountPercentage = activeDiscounts.Any() ? activeDiscounts.Max(d => d.Percentage) : 0m;
        
        // Add returning client discount (can be combined with other discounts)
        if (isReturningClient)
        {
            bestDiscountPercentage += 5m; // Additional 5% for returning clients
        }

        // Ensure discount doesn't exceed 100%
        bestDiscountPercentage = Math.Min(bestDiscountPercentage, 100m);

        var finalPrice = basePrice * (1 - bestDiscountPercentage / 100m);

        // 8. Create contract
        var contract = new Contract
        {
            ClientId = dto.ClientId,
            SoftwareId = dto.SoftwareId,
            SoftwareVersion = dto.SoftwareVersion,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Price = Math.Round(finalPrice, 2),
            AdditionalSupportYears = dto.AdditionalSupportYears,
            CreatedAt = DateTime.UtcNow,
            IsSigned = false,
            IsCancelled = false
        };

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        return await GetContract(contract.Id);
    }


    public async Task<bool> RemoveContract(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return false;

        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<List<int>> CancelExpiredContracts()
    {
        var expiredContracts = await _context.Contracts
            .Where(c => !c.IsSigned && 
                        !c.IsCancelled && 
                        c.EndDate < DateTime.UtcNow)
            .ToListAsync();

        var cancelledIds = new List<int>();

        foreach (var contract in expiredContracts)
        {
            contract.IsCancelled = true;
            cancelledIds.Add(contract.Id);
        }

        if (cancelledIds.Any())
        {
            await _context.SaveChangesAsync();
        }

        return cancelledIds;
    }
    
}