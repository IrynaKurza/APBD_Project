using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.DTOs.ClientDTOs;
using Project.Models;
using Project.Services.Services;

namespace Tests;

public class ClientServiceTests
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
        // Seed test data using the specific DbSets
        var individual = new IndividualClient
        {
            Id = 1,
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@test.com",
            PhoneNumber = "123456789",
            Address = "Test Address",
            PESEL = "80010112345",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var company = new CompanyClient
        {
            Id = 2,
            CompanyName = "Test Company",
            Email = "company@test.com",
            PhoneNumber = "987654321",
            Address = "Company Address",
            KRSNumber = "0000123456", // Use correct property name
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        context.IndividualClients.Add(individual);
        context.CompanyClients.Add(company);
        context.SaveChanges();
    }

    [Fact]
    public async Task CreateIndividualClient_ValidData_ReturnsClient()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new ClientService(context);

        var dto = new CreateIndividualClientDto
        {
            FirstName = "Anna",
            LastName = "Nowak",
            Email = "anna@test.com",
            PhoneNumber = "555666777",
            Address = "New Address",
            PESEL = "90010112345"
        };

        // Act
        var result = await service.CreateIndividualClient(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Anna Nowak", result.Name);
        Assert.Equal("Individual", result.Type);
        Assert.Equal("anna@test.com", result.Email);
    }

    [Fact]
    public async Task CreateCompanyClient_ValidData_ReturnsClient()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new ClientService(context);

        var dto = new CreateCompanyClientDto
        {
            CompanyName = "New Company",
            Email = "new@company.com",
            PhoneNumber = "555666777",
            Address = "New Address",
            KRSNumber = "0000999888" // Use correct property name
        };

        // Act
        var result = await service.CreateCompanyClient(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Company", result.Name);
        Assert.Equal("Company", result.Type);
        Assert.Equal("new@company.com", result.Email);
    }

    [Fact]
    public async Task GetClients_ReturnsAllActiveClients()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new ClientService(context);

        // Act
        var result = await service.GetClients();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name == "Jan Kowalski" && c.Type == "Individual");
        Assert.Contains(result, c => c.Name == "Test Company" && c.Type == "Company");
    }

    [Fact]
    public async Task GetClientById_ExistingId_ReturnsClient()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new ClientService(context);

        // Act
        var result = await service.GetClientById(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jan Kowalski", result.Name);
        Assert.Equal("Individual", result.Type);
    }

    [Fact]
    public async Task GetClientById_NonExistingId_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new ClientService(context);

        // Act
        var result = await service.GetClientById(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateIndividualClient_ValidData_ReturnsUpdatedClient()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new ClientService(context);

        var dto = new UpdateIndividualClientDto
        {
            Email = "updated@test.com",
            Address = "Updated Address"
        };

        // Act
        var result = await service.UpdateIndividualClient(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("updated@test.com", result.Email);
        Assert.Equal("Updated Address", result.Address);
        Assert.Equal("Jan Kowalski", result.Name); // Name should remain unchanged
    }

    [Fact]
    public async Task UpdateCompanyClient_ValidData_ReturnsUpdatedClient()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new ClientService(context);

        var dto = new UpdateCompanyClientDto
        {
            CompanyName = "Updated Company",
            Email = "updated@company.com"
        };

        // Act
        var result = await service.UpdateCompanyClient(2, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Company", result.Name);
        Assert.Equal("updated@company.com", result.Email);
    }

    [Fact]
    public async Task DeleteClient_IndividualClient_PerformsSoftDelete()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new ClientService(context);

        // Act
        var result = await service.DeleteClient(1); // Individual client

        // Assert
        Assert.True(result);
        
        // Verify client is marked as deleted and data is overwritten
        var deletedClient = await context.Set<Client>().FindAsync(1);
        Assert.NotNull(deletedClient);
        Assert.True(deletedClient.IsDeleted);
        
        var individualClient = deletedClient as IndividualClient;
        Assert.NotNull(individualClient);
        Assert.Equal("DELETED", individualClient.FirstName);
        Assert.Equal("DELETED", individualClient.LastName);
    }

    [Fact]
    public async Task DeleteClient_CompanyClient_ReturnsFalse()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new ClientService(context);

        // Act
        var result = await service.DeleteClient(2); // Company client

        // Assert - Company clients cannot be deleted
        Assert.False(result);
    }

    [Fact]
    public async Task IsReturningClient_ClientWithHistory_ReturnsTrue()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new ClientService(context);

        // Add a signed contract for client
        var contract = new Contract
        {
            ClientId = 1,
            SoftwareId = 1,
            SoftwareVersion = "1.0",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(-1),
            Price = 1000m,
            IsSigned = true,
            AdditionalSupportYears = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        // Act
        var result = await service.IsReturningClient(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsReturningClient_ClientWithoutHistory_ReturnsFalse()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new ClientService(context);

        // Act
        var result = await service.IsReturningClient(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetClients_OnlyReturnsNonDeletedClients()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var service = new ClientService(context);

        // Soft delete one client
        var client = await context.Set<Client>().FindAsync(1);
        if (client != null) client.IsDeleted = true;
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetClients();

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Company", result[0].Name);
    }
}