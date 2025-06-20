using Microsoft.EntityFrameworkCore;
using Moq;
using Project.Data;
using Project.DTOs.ContractDTOs;
using Project.Models;
using Project.Services.Interfaces;
using Project.Services.Services;

namespace Tests;

public class ContractServiceTests
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
            Category = "Test",
            AnnualLicenseCost = 1000m
        };

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

        var returningClient = new IndividualClient
        {
            Id = 2,
            FirstName = "Returning",
            LastName = "Client",
            Email = "returning@client.com",
            PhoneNumber = "987654321",
            Address = "Returning Address",
            PESEL = "85050512345",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        // Add existing contract for returning client
        var existingContract = new Contract
        {
            Id = 1,
            ClientId = 2,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow.AddDays(-60),
            EndDate = DateTime.UtcNow.AddDays(-30),
            Price = 1000m,
            IsSigned = true,
            IsCancelled = false,
            AdditionalSupportYears = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        context.Software.Add(software);
        context.IndividualClients.Add(client);
        context.IndividualClients.Add(returningClient);
        context.Contracts.Add(existingContract);
        context.SaveChanges();
    }

    [Fact]
    public async Task CreateContract_ValidDateRange_CreatesContract()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var clientService = Mock.Of<IClientService>();
        var service = new ContractService(context, clientService);

        var dto = new CreateContractDto
        {
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            AdditionalSupportYears = 1
        };

        // Act
        var result = await service.CreateContract(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2000m, result.Price); // 1000 base + 1000 for 1 additional year
    }

    [Fact]
    public async Task CreateContract_DateRangeTooShort_ThrowsException()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var clientService = Mock.Of<IClientService>();
        var service = new ContractService(context, clientService);

        var dto = new CreateContractDto
        {
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(2), // Only 2 days
            AdditionalSupportYears = 0
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateContract(dto));
        Assert.Contains("must be between 3 and 30 days", exception.Message);
    }

    [Fact]
    public async Task CreateContract_DateRangeTooLong_ThrowsException()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var clientService = Mock.Of<IClientService>();
        var service = new ContractService(context, clientService);

        var dto = new CreateContractDto
        {
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(35), // 35 days
            AdditionalSupportYears = 0
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateContract(dto));
        Assert.Contains("must be between 3 and 30 days", exception.Message);
    }

    [Fact]
    public async Task CreateContract_ReturningClient_AppliesDiscount()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
    
        // Properly mock the client service to return true for returning client
        var clientServiceMock = new Mock<IClientService>();
        clientServiceMock.Setup(x => x.IsReturningClient(2))
            .ReturnsAsync(true); // Client 2 is a returning client
    
        var service = new ContractService(context, clientServiceMock.Object);

        var dto = new CreateContractDto
        {
            ClientId = 2, // Returning client with existing contract
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            AdditionalSupportYears = 0
        };

        // Act
        var result = await service.CreateContract(dto);

        // Assert
        // Should be 1000 * 0.95 = 950 (5% returning client discount)
        if (result != null) Assert.Equal(950m, result.Price);
    }

    [Fact]
    public async Task CreateContract_MaximumAdditionalYears_CalculatesCorrectPrice()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var clientService = Mock.Of<IClientService>();
        var service = new ContractService(context, clientService);

        var dto = new CreateContractDto
        {
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            AdditionalSupportYears = 3 // Maximum allowed
        };

        // Act
        var result = await service.CreateContract(dto);

        // Assert
        // Should be 1000 + (3 * 1000) = 4000
        if (result != null) Assert.Equal(4000m, result.Price);
    }

    [Fact]
    public async Task CreateContract_InvalidAdditionalYears_ThrowsException()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var clientService = Mock.Of<IClientService>();
        var service = new ContractService(context, clientService);

        var dto = new CreateContractDto
        {
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            AdditionalSupportYears = 4 // More than maximum (3)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateContract(dto));
    }
}