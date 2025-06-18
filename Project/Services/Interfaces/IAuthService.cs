using Project.DTOs.AuthDTOs;

namespace Project.Services;

public interface IAuthService
{
    Task<AuthResponseDto> Register(RegisterDto dto);
    Task<AuthResponseDto> Login(LoginDto dto);
    Task<AuthResponseDto> RefreshToken(RefreshTokenDto dto);
}