using Project.DTOs.SoftwareDTOs;

namespace Project.Services.Interfaces;

public interface ISoftwareService
{
    Task<List<SoftwareResponseDto>> GetAllSoftware();
    Task<SoftwareResponseDto?> GetSoftwareById(int id);
    Task<SoftwareResponseDto> CreateSoftware(CreateSoftwareDto dto);
    Task<SoftwareResponseDto?> UpdateSoftware(int id, UpdateSoftwareDto dto);
    Task<bool> DeleteSoftware(int id);
    Task<List<SoftwareResponseDto>> GetSoftwareByCategory(string category);
}