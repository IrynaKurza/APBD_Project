using Project.DTOs.ContractDTOs;

namespace Project.Services.Interfaces;

public interface IContractService
{
    Task<List<ContractResponseDto>> GetContracts();
    Task<ContractResponseDto?> GetContract(int id);
    Task<ContractResponseDto?> CreateContract(CreateContractDto dto);
    Task<bool> RemoveContract(int id);
    Task<List<int>> CancelExpiredContracts();
}