using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.DTOs.AuthDTOs;
using Project.Models;
using Project.Services.Interfaces;

namespace Project.Services.Services;

public class AuthService : IAuthService
{
    private readonly DatabaseContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(DatabaseContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Register(RegisterDto dto)
    {
        // Check if user already exists
        var existingEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == dto.Email);
        
        if (existingEmployee != null)
            throw new InvalidOperationException("Employee with this email already exists");

        // Get role
        var role = await _context.EmployeeRoles
            .FirstOrDefaultAsync(r => r.Name == dto.Role);
        
        if (role == null)
            throw new ArgumentException("Invalid role");

        // Hash password
        var hashedPassword = new PasswordHasher<Employee>()
            .HashPassword(new Employee(), dto.Password);

        // Create employee
        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PasswordHash = hashedPassword,
            RoleId = role.Id
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Generate tokens
        var accessToken = _tokenService.CreateAccessToken(employee, role.Name);
        var refreshToken = _tokenService.CreateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            EmployeeId = employee.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Email = employee.Email,
            Role = role.Name
        };
    }

    public async Task<AuthResponseDto> Login(LoginDto dto)
    {
        // Find employee
        var employee = await _context.Employees
            .Include(e => e.Role)
            .FirstOrDefaultAsync(e => e.Email == dto.Email);

        if (employee == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        // Verify password
        var verificationResult = new PasswordHasher<Employee>()
            .VerifyHashedPassword(employee, employee.PasswordHash, dto.Password);

        if (verificationResult != PasswordVerificationResult.Success)
            throw new UnauthorizedAccessException("Invalid credentials");

        // Generate tokens
        var accessToken = _tokenService.CreateAccessToken(employee, employee.Role.Name);
        var refreshToken = _tokenService.CreateRefreshToken();

        // Update or create refresh token
        var existingRefreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.EmployeeId == employee.Id);

        if (existingRefreshToken != null)
        {
            existingRefreshToken.Token = refreshToken;
            existingRefreshToken.ExpiresAt = DateTime.UtcNow.AddDays(7);
        }
        else
        {
            var refreshTokenEntity = new RefreshToken
            {
                EmployeeId = employee.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            _context.RefreshTokens.Add(refreshTokenEntity);
        }

        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Email = employee.Email,
            Role = employee.Role.Name
        };
    }

    public async Task<AuthResponseDto> RefreshToken(RefreshTokenDto dto)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.Employee)
            .ThenInclude(e => e.Role)
            .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

        if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid refresh token");

        // Generate new tokens
        var accessToken = _tokenService.CreateAccessToken(refreshToken.Employee, refreshToken.Employee.Role.Name);
        var newRefreshToken = _tokenService.CreateRefreshToken();

        // Update refresh token
        refreshToken.Token = newRefreshToken;
        refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            Email = refreshToken.Employee.Email,
            Role = refreshToken.Employee.Role.Name
        };
    }
}