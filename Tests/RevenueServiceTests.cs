using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.DTOs.RevenueDTOs;
using Project.Models;
using Project.Services.Services;

namespace Tests;

public class RevenueServiceTests
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
        var software1 = new Software
        {
            Id = 1,
            Name = "Software 1",
            Description = "Test Software 1",
            CurrentVersion = "1.0",
            Category = "Test",
            AnnualLicenseCost = 1000m
        };

        var software2 = new Software
        {
            Id = 2,
            Name = "Software 2",
            Description = "Test Software 2",
            CurrentVersion = "1.0",
            Category = "Test",
            AnnualLicenseCost = 2000m
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

        // Signed contract (contributes to current revenue)
        var signedContract = new Contract
        {
            Id = 1,
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow.AddDays(-20),
            EndDate = DateTime.UtcNow.AddDays(-10),
            Price = 1000m,
            IsSigned = true,
            IsCancelled = false,
            AdditionalSupportYears = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        // Unsigned contract (contributes to predicted revenue)
        var unsignedContract = new Contract
        {
            Id = 2,
            ClientId = 1,
            SoftwareId = 2,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Price = 2000m,
            IsSigned = false,
            IsCancelled = false,
            AdditionalSupportYears = 0,
            CreatedAt = DateTime.UtcNow
        };

        // Cancelled contract (should not contribute to any revenue)
        var cancelledContract = new Contract
        {
            Id = 3,
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(-20),
            Price = 500m,
            IsSigned = false,
            IsCancelled = true,
            AdditionalSupportYears = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        context.Software.AddRange(software1, software2);
        context.IndividualClients.Add(client); // FIXED: Use specific DbSet
        context.Contracts.AddRange(signedContract, unsignedContract, cancelledContract);
        context.SaveChanges();
    }

    [Fact]
    public async Task CalculateRevenue_AllSoftware_PLN_ReturnsCorrectRevenue()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new RevenueService(context);

        var query = new RevenueQueryDto
        {
            SoftwareId = null,
            Currency = "PLN"
        };

        // Act
        var result = await service.CalculateRevenue(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000m, result.Amount); // Only signed contracts count as current revenue
        Assert.Equal("PLN", result.Currency);
    }

    [Fact]
    public async Task CalculateRevenue_SpecificSoftware_ReturnsCorrectRevenue()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new RevenueService(context);

        var query = new RevenueQueryDto
        {
            SoftwareId = 1,
            Currency = "PLN"
        };

        // Act
        var result = await service.CalculateRevenue(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000m, result.Amount); // Only software 1 signed contracts
        Assert.Equal("PLN", result.Currency);
    }

    [Fact]
    public async Task CalculateRevenue_USD_AppliesExchangeRate()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new RevenueService(context);

        var query = new RevenueQueryDto
        {
            SoftwareId = null,
            Currency = "USD"
        };

        // Act
        var result = await service.CalculateRevenue(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(250m, result.Amount); // 1000 PLN * 0.25 USD rate
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task CalculateRevenue_EUR_AppliesExchangeRate()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new RevenueService(context);

        var query = new RevenueQueryDto
        {
            SoftwareId = null,
            Currency = "EUR"
        };

        // Act
        var result = await service.CalculateRevenue(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(230m, result.Amount); // 1000 PLN * 0.23 EUR rate
        Assert.Equal("EUR", result.Currency);
    }

    [Fact]
    public async Task CalculateRevenue_UnknownCurrency_UsesPLN()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new RevenueService(context);

        var query = new RevenueQueryDto
        {
            SoftwareId = null,
            Currency = "JPY" // Not implemented currency
        };

        // Act
        var result = await service.CalculateRevenue(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000m, result.Amount); // No conversion applied
        Assert.Equal("JPY", result.Currency);
    }
}

public class PredictedRevenueTests
{
    private DatabaseContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new DatabaseContext(options);
        SeedTestData(context);
        return context;
    }

    private void SeedTestData(DatabaseContext context)
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

        // Signed contract (counts toward current revenue)
        var signedContract = new Contract
        {
            Id = 1,
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-5),
            Price = 2000m,
            IsSigned = true,      // Signed
            IsCancelled = false,
            AdditionalSupportYears = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        // Unsigned contract (active deadline - counts toward predicted revenue)
        var unsignedActiveContract = new Contract
        {
            Id = 2,
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10), // Still has time to pay
            Price = 3000m,
            IsSigned = false,     // Not signed yet
            IsCancelled = false,
            AdditionalSupportYears = 0,
            CreatedAt = DateTime.UtcNow
        };

        // Unsigned expired contract (should NOT count toward any revenue)
        var unsignedExpiredContract = new Contract
        {
            Id = 3,
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow.AddDays(-20),
            EndDate = DateTime.UtcNow.AddDays(-5), // Deadline passed
            Price = 1500m,
            IsSigned = false,     // Not signed
            IsCancelled = false,  // But deadline passed
            AdditionalSupportYears = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        // Cancelled contract (should NOT count toward any revenue)
        var cancelledContract = new Contract
        {
            Id = 4,
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow.AddDays(-15),
            EndDate = DateTime.UtcNow.AddDays(-10),
            Price = 1000m,
            IsSigned = false,
            IsCancelled = true,   // Cancelled
            AdditionalSupportYears = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        };

        context.Software.Add(software);
        context.IndividualClients.Add(client);
        context.Contracts.AddRange(signedContract, unsignedActiveContract, unsignedExpiredContract, cancelledContract);
        context.SaveChanges();
    }

    [Fact]
    public async Task CalculateRevenue_Current_OnlyCountsSignedContracts()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new RevenueService(context);

        var query = new RevenueQueryDto
        {
            RevenueType = "Current",
            Currency = "PLN"
        };

        // Act
        var result = await service.CalculateRevenue(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2000m, result.Amount); // Only signed contract (2000 PLN)
        Assert.Equal("Current", result.CalculationType);
        Assert.Equal("PLN", result.Currency);
    }

    [Fact]
    public async Task CalculateRevenue_Predicted_CountsSignedAndUnsignedActive()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new RevenueService(context);

        var query = new RevenueQueryDto
        {
            RevenueType = "Predicted",
            Currency = "PLN"
        };

        // Act
        var result = await service.CalculateRevenue(query);

        // Assert
        Assert.NotNull(result);
        // Should be: 2000 (signed) + 3000 (unsigned active) = 5000
        // Should NOT include: 1500 (expired) + 1000 (cancelled)
        Assert.Equal(5000m, result.Amount);
        Assert.Equal("Predicted", result.CalculationType);
        Assert.Equal("PLN", result.Currency);
    }

    [Fact]
    public async Task CalculateRevenue_Predicted_WithCurrency_AppliesExchangeRate()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new RevenueService(context);

        var query = new RevenueQueryDto
        {
            RevenueType = "Predicted",
            Currency = "USD"
        };

        // Act
        var result = await service.CalculateRevenue(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1250m, result.Amount); // 5000 PLN * 0.25 USD rate = 1250 USD
        Assert.Equal("Predicted", result.CalculationType);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task CalculateRevenue_DefaultType_ReturnsCurrentRevenue()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new RevenueService(context);

        var query = new RevenueQueryDto
        {
            // RevenueType not specified - should default to "Current"
            Currency = "PLN"
        };

        // Act
        var result = await service.CalculateRevenue(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2000m, result.Amount); // Only signed contracts
        Assert.Equal("Current", result.CalculationType);
    }

    [Fact]
    public async Task CalculateRevenue_SpecificSoftware_FiltersCorrectly()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new RevenueService(context);

        var query = new RevenueQueryDto
        {
            SoftwareId = 1,
            RevenueType = "Predicted",
            Currency = "PLN"
        };

        // Act
        var result = await service.CalculateRevenue(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5000m, result.Amount); // Same result since all contracts are for software ID 1
        Assert.Equal("Predicted", result.CalculationType);
    }
}