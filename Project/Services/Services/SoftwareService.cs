using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.DTOs.SoftwareDTOs;
using Project.Models;
using Project.Services.Interfaces;

namespace Project.Services.Services;

public class SoftwareService : ISoftwareService
{
    private readonly DatabaseContext _context;
    private readonly ILogger<SoftwareService> _logger;

    public SoftwareService(DatabaseContext context, ILogger<SoftwareService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<SoftwareResponseDto>> GetAllSoftware()
    {
        var software = await _context.Software
            .Select(s => new SoftwareResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                CurrentVersion = s.CurrentVersion,
                Category = s.Category,
                AnnualLicenseCost = s.AnnualLicenseCost,
                ActiveContractsCount = s.Contracts.Count(c => !c.IsCancelled),
                TotalRevenue = s.Contracts.Where(c => c.IsSigned).SelectMany(c => c.Payments).Sum(p => p.Amount)
            })
            .ToListAsync();

        return software;
    }

    public async Task<SoftwareResponseDto?> GetSoftwareById(int id)
    {
        var software = await _context.Software
            .Where(s => s.Id == id)
            .Select(s => new SoftwareResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                CurrentVersion = s.CurrentVersion,
                Category = s.Category,
                AnnualLicenseCost = s.AnnualLicenseCost,
                ActiveContractsCount = s.Contracts.Count(c => !c.IsCancelled),
                TotalRevenue = s.Contracts.Where(c => c.IsSigned).SelectMany(c => c.Payments).Sum(p => p.Amount)
            })
            .FirstOrDefaultAsync();

        return software;
    }

    public async Task<SoftwareResponseDto> CreateSoftware(CreateSoftwareDto dto)
    {
        // Check if software with same name already exists
        var existingSoftware = await _context.Software
            .FirstOrDefaultAsync(s => s.Name.ToLower() == dto.Name.ToLower());

        if (existingSoftware != null)
            throw new InvalidOperationException($"Software with name '{dto.Name}' already exists");

        var software = new Software
        {
            Name = dto.Name,
            Description = dto.Description,
            CurrentVersion = dto.CurrentVersion,
            Category = dto.Category,
            AnnualLicenseCost = dto.AnnualLicenseCost
        };

        _context.Software.Add(software);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new software: {SoftwareName} (ID: {SoftwareId})", software.Name, software.Id);

        return new SoftwareResponseDto
        {
            Id = software.Id,
            Name = software.Name,
            Description = software.Description,
            CurrentVersion = software.CurrentVersion,
            Category = software.Category,
            AnnualLicenseCost = software.AnnualLicenseCost,
            ActiveContractsCount = 0,
            TotalRevenue = 0
        };
    }

    public async Task<SoftwareResponseDto?> UpdateSoftware(int id, UpdateSoftwareDto dto)
    {
        var software = await _context.Software.FindAsync(id);
        if (software == null) return null;

        // Update only provided fields
        if (!string.IsNullOrEmpty(dto.Description))
            software.Description = dto.Description;

        if (!string.IsNullOrEmpty(dto.CurrentVersion))
            software.CurrentVersion = dto.CurrentVersion;

        if (dto.AnnualLicenseCost.HasValue)
            software.AnnualLicenseCost = dto.AnnualLicenseCost.Value;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated software: {SoftwareName} (ID: {SoftwareId})", software.Name, software.Id);

        return await GetSoftwareById(id);
    }

    public async Task<bool> DeleteSoftware(int id)
    {
        var software = await _context.Software
            .Include(s => s.Contracts)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (software == null) return false;

        // Check if software has active contracts
        var hasActiveContracts = software.Contracts.Any(c => !c.IsCancelled);
        if (hasActiveContracts)
            throw new InvalidOperationException("Cannot delete software with active contracts");

        _context.Software.Remove(software);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted software: {SoftwareName} (ID: {SoftwareId})", software.Name, software.Id);

        return true;
    }

    public async Task<List<SoftwareResponseDto>> GetSoftwareByCategory(string category)
    {
        var software = await _context.Software
            .Where(s => s.Category.ToLower() == category.ToLower())
            .Select(s => new SoftwareResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                CurrentVersion = s.CurrentVersion,
                Category = s.Category,
                AnnualLicenseCost = s.AnnualLicenseCost,
                ActiveContractsCount = s.Contracts.Count(c => !c.IsCancelled),
                TotalRevenue = s.Contracts.Where(c => c.IsSigned).SelectMany(c => c.Payments).Sum(p => p.Amount)
            })
            .ToListAsync();

        return software;
    }
}