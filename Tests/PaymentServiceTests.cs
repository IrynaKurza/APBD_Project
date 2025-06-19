using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.DTOs.PaymentDTOs;
using Project.Models;
using Project.Services.Services;

namespace Tests;

public class PaymentServiceTests
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
            IsCancelled = false,
            AdditionalSupportYears = 0,
            CreatedAt = DateTime.UtcNow
        };

        var expiredContract = new Contract
        {
            Id = 2,
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow.AddDays(-20),
            EndDate = DateTime.UtcNow.AddDays(-1), // Expired
            Price = 1000m,
            IsSigned = false,
            IsCancelled = false,
            AdditionalSupportYears = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        var partiallyPaidContract = new Contract
        {
            Id = 3,
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Price = 2000m,
            IsSigned = false,
            IsCancelled = false,
            AdditionalSupportYears = 0,
            CreatedAt = DateTime.UtcNow
        };

        var existingPayment = new Payment
        {
            Id = 1,
            ContractId = 3,
            Amount = 800m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "Credit Card"
        };

        context.IndividualClients.Add(client);
        context.Contracts.AddRange(contract, expiredContract, partiallyPaidContract);
        context.Payments.Add(existingPayment);
        context.SaveChanges();
    }

    [Fact]
    public async Task CreatePayment_ValidFullPayment_SignsContract()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new PaymentService(context);

        var dto = new CreatePaymentDto
        {
            ContractId = 1,
            Amount = 1000m,
            PaymentMethod = "Credit Card"
        };

        // Act
        var result = await service.CreatePayment(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000m, result.Amount);
        Assert.True(result.ContractFullyPaid);
        Assert.Equal(0m, result.RemainingBalance);

        // Verify contract is signed
        var contract = await context.Contracts.FindAsync(1);
        Assert.True(contract != null && contract.IsSigned);
    }

    [Fact]
    public async Task CreatePayment_ValidPartialPayment_DoesNotSignContract()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new PaymentService(context);

        var dto = new CreatePaymentDto
        {
            ContractId = 1,
            Amount = 500m,
            PaymentMethod = "Bank Transfer"
        };

        // Act
        var result = await service.CreatePayment(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500m, result.Amount);
        Assert.False(result.ContractFullyPaid);
        Assert.Equal(500m, result.RemainingBalance);

        // Verify contract is not signed
        var contract = await context.Contracts.FindAsync(1);
        Assert.False(contract != null && contract.IsSigned);
    }

    [Fact]
    public async Task CreatePayment_ExceedsContractAmount_ThrowsException()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new PaymentService(context);

        var dto = new CreatePaymentDto
        {
            ContractId = 1,
            Amount = 1500m, // More than contract price (1000)
            PaymentMethod = "Credit Card"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreatePayment(dto));
        Assert.Contains("exceeds remaining balance", exception.Message);
    }

    [Fact]
    public async Task CreatePayment_ExpiredContract_CancelsContractAndThrowsException()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new PaymentService(context);

        var dto = new CreatePaymentDto
        {
            ContractId = 2, // Expired contract
            Amount = 500m,
            PaymentMethod = "Credit Card"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreatePayment(dto));
        Assert.Contains("Payment deadline exceeded", exception.Message);

        // Verify contract is cancelled
        var contract = await context.Contracts.FindAsync(2);
        Assert.True(contract != null && contract.IsCancelled);
    }

    [Fact]
    public async Task CreatePayment_InvalidPaymentMethod_ThrowsException()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new PaymentService(context);

        var dto = new CreatePaymentDto
        {
            ContractId = 1,
            Amount = 500m,
            PaymentMethod = "Cryptocurrency" // Invalid payment method
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreatePayment(dto));
        Assert.Contains("Invalid payment method", exception.Message);
    }

    [Fact]
    public async Task CreatePayment_CompletesPartiallyPaidContract_SignsContract()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new PaymentService(context);

        var dto = new CreatePaymentDto
        {
            ContractId = 3, // Contract with 800 already paid, needs 1200 more
            Amount = 1200m,
            PaymentMethod = "Bank Transfer"
        };

        // Act
        var result = await service.CreatePayment(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1200m, result.Amount);
        Assert.True(result.ContractFullyPaid);
        Assert.Equal(0m, result.RemainingBalance);

        // Verify contract is signed
        var contract = await context.Contracts.FindAsync(3);
        Assert.True(contract != null && contract.IsSigned);
    }

    [Fact]
    public async Task ValidatePayment_ValidAmount_ReturnsValidation()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new PaymentService(context);

        // Act
        var result = await service.ValidatePayment(1, 500m);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.Equal(1000m, result.RemainingBalance); // FIXED: Use RemainingBalance instead of RemainingAfterPayment
    }

    [Fact]
    public async Task ValidatePayment_ExcessiveAmount_ReturnsInvalid()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new PaymentService(context);

        // Act
        var result = await service.ValidatePayment(1, 1500m);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Contains("exceeds", result.ErrorMessage);
    }
}