using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Data;
using Project.DTOs.SoftwareDTOs;
using Project.Models;
using Project.Services.Services;

namespace Tests;

public class SoftwareServiceTests
{
    private DatabaseContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new DatabaseContext(options);
        SeedDatabase(context);
        return context;
    }

    private void SeedDatabase(DatabaseContext context)
    {
        var software = new Software
        {
            Id = 1,
            Name = "Test Software",
            Description = "Test Description",
            CurrentVersion = "1.0",
            Category = "Productivity",
            AnnualLicenseCost = 1000m
        };

        context.Software.Add(software);
        context.SaveChanges();
    }

    [Fact]
    public async Task CreateSoftware_ValidData_CreatesSoftware()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = Mock.Of<ILogger<SoftwareService>>();
        var service = new SoftwareService(context, logger);

        var dto = new CreateSoftwareDto
        {
            Name = "New Software",
            Description = "New Description",
            CurrentVersion = "2.0",
            Category = "Design",
            AnnualLicenseCost = 2000m
        };

        // Act
        var result = await service.CreateSoftware(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Software", result.Name);
        Assert.Equal("Design", result.Category);
        Assert.Equal(2000m, result.AnnualLicenseCost);
    }

    [Fact]
    public async Task CreateSoftware_DuplicateName_ThrowsException()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = Mock.Of<ILogger<SoftwareService>>();
        var service = new SoftwareService(context, logger);

        var dto = new CreateSoftwareDto
        {
            Name = "Test Software", // Same as seeded data
            Description = "Duplicate Description",
            CurrentVersion = "2.0",
            Category = "Test",
            AnnualLicenseCost = 1500m
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateSoftware(dto));
    }

    [Fact]
    public async Task GetSoftwareByCategory_ReturnsCorrectSoftware()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = Mock.Of<ILogger<SoftwareService>>();
        var service = new SoftwareService(context, logger);

        // Act
        var result = await service.GetSoftwareByCategory("Productivity");

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Software", result[0].Name);
    }

    [Fact]
    public async Task GetSoftwareByCategory_NonExistentCategory_ReturnsEmpty()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = Mock.Of<ILogger<SoftwareService>>();
        var service = new SoftwareService(context, logger);

        // Act
        var result = await service.GetSoftwareByCategory("NonExistent");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteSoftware_WithActiveContracts_ThrowsException()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = Mock.Of<ILogger<SoftwareService>>();
        var service = new SoftwareService(context, logger);

        // Add active contract
        var client = new IndividualClient
        {
            Id = 1,
            FirstName = "Test",
            LastName = "Client",
            Email = "test@client.com",
            PhoneNumber = "123456789",
            Address = "Test Address",
            PESEL = "80010112345",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var contract = new Contract
        {
            Id = 1,
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Price = 1000m,
            IsSigned = false,
            IsCancelled = false, // Active contract
            AdditionalSupportYears = 0,
            CreatedAt = DateTime.UtcNow
        };

        context.IndividualClients.Add(client); // FIXED: Use specific DbSet
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteSoftware(1));
    }
}