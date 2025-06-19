using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.DTOs.ClientDTOs;
using Project.Models;
using Project.Services.Interfaces;

namespace Project.Services.Services;

public class ClientService : IClientService
{
    private readonly DatabaseContext _context;

    public ClientService(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<ClientResponseDto>> GetClients()
    {
        var clients = await _context.Set<Client>()
            .Where(c => !c.IsDeleted)
            .Select(c => new ClientResponseDto
            {
                Id = c.Id,
                Type = c is IndividualClient ? "Individual" : "Company",
                Name = c is IndividualClient 
                    ? ((IndividualClient)c).FirstName + " " + ((IndividualClient)c).LastName
                    : ((CompanyClient)c).CompanyName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address
            }).ToListAsync();

        return clients;
    }

    public async Task<ClientResponseDto?> GetClientById(int id)
    {
        var client = await _context.Set<Client>()
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => new ClientResponseDto
            {
                Id = c.Id,
                Type = c is IndividualClient ? "Individual" : "Company",
                Name = c is IndividualClient 
                    ? ((IndividualClient)c).FirstName + " " + ((IndividualClient)c).LastName
                    : ((CompanyClient)c).CompanyName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address
            }).FirstOrDefaultAsync();

        return client;
    }

    public async Task<ClientResponseDto> CreateIndividualClient(CreateIndividualClientDto dto)
    {
        var client = new IndividualClient
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PESEL = dto.PESEL,
            Address = dto.Address,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        _context.IndividualClients.Add(client);
        await _context.SaveChangesAsync();

        return new ClientResponseDto
        {
            Id = client.Id,
            Type = "Individual",
            Name = client.FirstName + " " + client.LastName,
            Email = client.Email,
            PhoneNumber = client.PhoneNumber,
            Address = client.Address
        };
    }

    public async Task<ClientResponseDto> CreateCompanyClient(CreateCompanyClientDto dto)
    {
        var client = new CompanyClient
        {
            CompanyName = dto.CompanyName,
            KRSNumber = dto.KRSNumber,
            Address = dto.Address,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        _context.CompanyClients.Add(client);
        await _context.SaveChangesAsync();

        return new ClientResponseDto
        {
            Id = client.Id,
            Type = "Company",
            Name = client.CompanyName,
            Email = client.Email,
            PhoneNumber = client.PhoneNumber,
            Address = client.Address
        };
    }

    public async Task<bool> DeleteClient(int id)
    {
        var client = await _context.Set<Client>().FindAsync(id);
        
        if (client == null || client is not IndividualClient) 
            return false;

        client.IsDeleted = true;
        var individual = (IndividualClient)client;
        individual.FirstName = "DELETED";
        individual.LastName = "DELETED";
        individual.Email = "DELETED";
        individual.PhoneNumber = "DELETED";
        individual.Address = "DELETED";

        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<ClientResponseDto?> UpdateIndividualClient(int id, UpdateIndividualClientDto dto)
    {
        var client = await _context.Set<IndividualClient>()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (client == null) return null;

        // Update only provided fields
        if (!string.IsNullOrEmpty(dto.Email))
            client.Email = dto.Email;

        if (!string.IsNullOrEmpty(dto.PhoneNumber))
            client.PhoneNumber = dto.PhoneNumber;

        if (!string.IsNullOrEmpty(dto.Address))
            client.Address = dto.Address;

        if (!string.IsNullOrEmpty(dto.FirstName))
            client.FirstName = dto.FirstName;

        if (!string.IsNullOrEmpty(dto.LastName))
            client.LastName = dto.LastName;

        // Note: PESEL cannot be updated per business requirements

        await _context.SaveChangesAsync();

        return new ClientResponseDto
        {
            Id = client.Id,
            Type = "Individual",
            Name = $"{client.FirstName} {client.LastName}",
            Email = client.Email,
            PhoneNumber = client.PhoneNumber,
            Address = client.Address
        };
    }

    public async Task<ClientResponseDto?> UpdateCompanyClient(int id, UpdateCompanyClientDto dto)
    {
        var client = await _context.Set<CompanyClient>()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (client == null) return null;

        // Update only provided fields
        if (!string.IsNullOrEmpty(dto.Email))
            client.Email = dto.Email;

        if (!string.IsNullOrEmpty(dto.PhoneNumber))
            client.PhoneNumber = dto.PhoneNumber;

        if (!string.IsNullOrEmpty(dto.Address))
            client.Address = dto.Address;

        if (!string.IsNullOrEmpty(dto.CompanyName))
            client.CompanyName = dto.CompanyName;

        // Note: KRS number cannot be updated per business requirements

        await _context.SaveChangesAsync();

        return new ClientResponseDto
        {
            Id = client.Id,
            Type = "Company",
            Name = client.CompanyName,
            Email = client.Email,
            PhoneNumber = client.PhoneNumber,
            Address = client.Address
        };
    }

    public async Task<bool> IsReturningClient(int clientId)
    {
        var hasSignedContract = await _context.Contracts
            .AnyAsync(c => c.ClientId == clientId && c.IsSigned);
    
        return hasSignedContract;
    }
}