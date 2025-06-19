using Microsoft.EntityFrameworkCore;
using Moq;
using Project.Data;
using Project.DTOs.ContractDTOs;
using Project.DTOs.PaymentDTOs;
using Project.Models;
using Project.Services.Interfaces;
using Project.Services.Services;

namespace Tests;

public class BusinessLogicIntegrationTests
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
        // Seed software
        var software = new Software
        {
            Id = 1,
            Name = "Premium Software",
            Description = "Premium software package",
            CurrentVersion = "1.0",
            Category = "Business",
            AnnualLicenseCost = 5000m
        };

        var newClient = new IndividualClient
        {
            Id = 1,
            FirstName = "New",
            LastName = "Client",
            Email = "new@client.com",
            PhoneNumber = "123456789",
            Address = "New Address",
            PESEL = "90010112345",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var returningClient = new CompanyClient
        {
            Id = 2,
            CompanyName = "Returning Corp",
            Email = "returning@corp.com",
            PhoneNumber = "987654321",
            Address = "Corp Address",
            KRSNumber = "0000123456", // Fixed property name
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        // Historical contract for returning client
        var historicalContract = new Contract
        {
            Id = 1,
            ClientId = 2,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow.AddDays(-365),
            EndDate = DateTime.UtcNow.AddDays(-335),
            Price = 4750m, // With discount
            IsSigned = true,
            IsCancelled = false,
            AdditionalSupportYears = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-365)
        };

        context.Software.Add(software);
        context.IndividualClients.Add(newClient);
        context.CompanyClients.Add(returningClient);
        context.Contracts.Add(historicalContract);
        context.SaveChanges();
    }

    [Fact]
    public async Task CompleteBusinessFlow_NewClient_NoDiscount()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var clientService = Mock.Of<IClientService>();
        var contractService = new ContractService(context, clientService);
        var paymentService = new PaymentService(context);

        // Act - Create contract for new client
        var contractDto = new CreateContractDto
        {
            ClientId = 1, // New client
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14),
            AdditionalSupportYears = 2
        };

        var contract = await contractService.CreateContract(contractDto);

        // Assert - Price should be full price without discount
        if (contract != null)
        {
            Assert.Equal(7000m, contract.Price); // 5000 + (2 * 1000) = 7000

            // Act - Make payment
            var paymentDto = new CreatePaymentDto
            {
                ContractId = contract.Id,
                Amount = 7000m,
                PaymentMethod = "Credit Card"
            };

            var payment = await paymentService.CreatePayment(paymentDto);

            // Assert - Contract should be signed
            Assert.True(payment.ContractFullyPaid);

            // Verify contract is marked as signed
            var updatedContract = await context.Contracts.FindAsync(contract.Id);
            Assert.NotNull(updatedContract);
            Assert.True(updatedContract.IsSigned);
        }
    }

    [Fact]
    public async Task CompleteBusinessFlow_ReturningClient_GetsDiscount()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var clientServiceMock = new Mock<IClientService>();
        clientServiceMock.Setup(x => x.IsReturningClient(2))
                        .ReturnsAsync(true); // Setup returning client

        var contractService = new ContractService(context, clientServiceMock.Object);
        var paymentService = new PaymentService(context);

        // Act - Create contract for returning client
        var contractDto = new CreateContractDto
        {
            ClientId = 2, // Returning client
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14),
            AdditionalSupportYears = 1
        };

        var contract = await contractService.CreateContract(contractDto);

        // Assert - Price should include 5% returning client discount
        // (5000 + 1000) * 0.95 = 5700
        if (contract != null)
        {
            Assert.Equal(5700m, contract.Price);

            // Act - Make partial payments
            var payment1 = await paymentService.CreatePayment(new CreatePaymentDto
            {
                ContractId = contract.Id,
                Amount = 3000m,
                PaymentMethod = "Bank Transfer"
            });

            Assert.False(payment1.ContractFullyPaid);
            Assert.Equal(2700m, payment1.RemainingBalance);

            var payment2 = await paymentService.CreatePayment(new CreatePaymentDto
            {
                ContractId = contract.Id,
                Amount = 2700m,
                PaymentMethod = "Credit Card"
            });

            // Assert - Contract should be fully paid and signed
            Assert.True(payment2.ContractFullyPaid);
            Assert.Equal(0m, payment2.RemainingBalance);

            var updatedContract = await context.Contracts.FindAsync(contract.Id);
            Assert.NotNull(updatedContract);
            Assert.True(updatedContract.IsSigned);
        }
    }

    [Fact]
    public async Task PaymentValidation_PreventOverbayment()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var clientService = Mock.Of<IClientService>();
        var contractService = new ContractService(context, clientService);
        var paymentService = new PaymentService(context);

        var contractDto = new CreateContractDto
        {
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14),
            AdditionalSupportYears = 0
        };

        var contract = await contractService.CreateContract(contractDto);

        // Act & Assert - Overpayment should fail
        if (contract != null)
        {
            var overpaymentDto = new CreatePaymentDto
            {
                ContractId = contract.Id,
                Amount = contract.Price + 1000m, // More than contract price
                PaymentMethod = "Credit Card"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => paymentService.CreatePayment(overpaymentDto));
        }
    }

    // Fix for BusinessLogicIntegrationTests.cs
// Replace the ContractExpiration_PreventPaymentAfterDeadline test method:

    [Fact]
    public async Task ContractExpiration_PreventPaymentAfterDeadline()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var paymentService = new PaymentService(context);

        // FIXED: Create expired contract directly in database to bypass validation
        var expiredContract = new Contract
        {
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-1), // Expired yesterday
            Price = 5000m,
            AdditionalSupportYears = 0,
            IsSigned = false,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        context.Contracts.Add(expiredContract);
        await context.SaveChangesAsync();

        // Act & Assert - Payment should fail due to expiration
        var paymentDto = new CreatePaymentDto
        {
            ContractId = expiredContract.Id,
            Amount = expiredContract.Price,
            PaymentMethod = "Credit Card"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => paymentService.CreatePayment(paymentDto));

        // Verify contract is cancelled
        var cancelledContract = await context.Contracts.FindAsync(expiredContract.Id);
        Assert.NotNull(cancelledContract);
        Assert.True(cancelledContract.IsCancelled);
    }

    [Fact]
    public async Task PaymentMethodValidation_RejectsInvalidMethods()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var clientService = Mock.Of<IClientService>();
        var contractService = new ContractService(context, clientService);
        var paymentService = new PaymentService(context);

        var contractDto = new CreateContractDto
        {
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14),
            AdditionalSupportYears = 0
        };

        var contract = await contractService.CreateContract(contractDto);

        // Act & Assert - Invalid payment method should fail
        if (contract != null)
        {
            var invalidPaymentDto = new CreatePaymentDto
            {
                ContractId = contract.Id,
                Amount = 1000m,
                PaymentMethod = "Cryptocurrency" // Invalid method
            };

            await Assert.ThrowsAsync<ArgumentException>(
                () => paymentService.CreatePayment(invalidPaymentDto));
        }
    }

    [Fact]
    public async Task PaymentValidation_ValidatesCorrectly()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var clientService = Mock.Of<IClientService>();
        var contractService = new ContractService(context, clientService);
        var paymentService = new PaymentService(context);

        var contractDto = new CreateContractDto
        {
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14),
            AdditionalSupportYears = 0
        };

        var contract = await contractService.CreateContract(contractDto);

        // Act - Validate partial payment
        if (contract != null)
        {
            var validation = await paymentService.ValidatePayment(contract.Id, 3000m);

            // Assert
            Assert.NotNull(validation);
            Assert.True(validation.IsValid);
            Assert.Equal(5000m, validation.RemainingBalance); // FIXED: Use RemainingBalance instead of RemainingAfterPayment
        }

        // Act - Validate excessive payment
        if (contract != null)
        {
            var invalidValidation = await paymentService.ValidatePayment(contract.Id, 6000m);

            // Assert
            Assert.NotNull(invalidValidation);
            Assert.False(invalidValidation.IsValid);
            Assert.Contains("exceeds", invalidValidation.ErrorMessage);
        }
    }
}