using Project.Models;

namespace Project.Services.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(Employee employee, string roleName);
    string CreateRefreshToken();
}