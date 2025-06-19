using Project.DTOs.ClientDTOs;

namespace Project.Services.Interfaces;

public interface IClientService
{
    Task<List<ClientResponseDto>> GetClients();
    Task<ClientResponseDto?> GetClientById(int id);
    Task<ClientResponseDto> CreateIndividualClient(CreateIndividualClientDto dto);
    Task<ClientResponseDto> CreateCompanyClient(CreateCompanyClientDto dto);
    Task<bool> DeleteClient(int id);
    Task<bool> IsReturningClient(int clientId);
    Task<ClientResponseDto?> UpdateIndividualClient(int id, UpdateIndividualClientDto dto);
    Task<ClientResponseDto?> UpdateCompanyClient(int id, UpdateCompanyClientDto dto);
}