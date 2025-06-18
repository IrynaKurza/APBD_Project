namespace Project.DTOs.AuthDTOs;

public class AuthResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}