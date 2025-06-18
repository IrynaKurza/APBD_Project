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

    public async Task<ContractResponseDto> GetContract(int id)
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

    public async Task<ContractResponseDto> CreateContract(CreateContractDto dto)
    {
        var software = await _context.Software.FindAsync(dto.SoftwareId);
        if (software == null) throw new ArgumentException("Software not found");

        // Check for active contracts
        var hasActiveContract = await _context.Contracts
            .AnyAsync(c => c.ClientId == dto.ClientId && 
                          c.SoftwareId == dto.SoftwareId && 
                          !c.IsCancelled);

        if (hasActiveContract) 
            throw new InvalidOperationException("Client already has active contract for this software");

        // Calculate price
        var basePrice = software.AnnualLicenseCost + (dto.AdditionalSupportYears * 1000m);
        var isReturningClient = await _clientService.IsReturningClient(dto.ClientId);
        
        // Apply discounts
        var discountPercentage = 0m;
        var activeDiscounts = await _context.Discounts
            .Where(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)
            .ToListAsync();

        if (activeDiscounts.Any())
        {
            discountPercentage = activeDiscounts.Max(d => d.Percentage);
        }

        if (isReturningClient)
        {
            discountPercentage += 5m; // Additional 5% for returning clients
        }

        var finalPrice = basePrice * (1 - discountPercentage / 100m);

        var contract = new Contract
        {
            ClientId = dto.ClientId,
            SoftwareId = dto.SoftwareId,
            SoftwareVersion = dto.SoftwareVersion,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Price = finalPrice,
            AdditionalSupportYears = dto.AdditionalSupportYears,
            CreatedAt = DateTime.UtcNow
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
}