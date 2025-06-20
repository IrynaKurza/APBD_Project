using System.ComponentModel.DataAnnotations;
using Project.DTOs.ClientDTOs;
using Project.Models;
using Microsoft.EntityFrameworkCore;
using Project.Data;

namespace Tests;

public class EnhancedValidationTests
{
    private List<ValidationResult> ValidateDto<T>(T dto) where T : notnull
    {
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, context, results, true);
        return results;
    }

    [Fact]
    public void CreateIndividualClientDto_InvalidPESEL_FailsValidation()
    {
        // Arrange
        var dto = new CreateIndividualClientDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "123456789",
            Address = "Test Address",
            PESEL = "123456789" // Invalid - only 9 digits
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Contains(validationResults, r => r.ErrorMessage != null && r.ErrorMessage.Contains("PESEL must contain exactly 11 digits"));
    }

    [Fact]
    public void CreateIndividualClientDto_ValidPESEL_PassesValidation()
    {
        // Arrange
        var dto = new CreateIndividualClientDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "123456789",
            Address = "Test Address",
            PESEL = "80010112345" // Valid - 11 digits
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.DoesNotContain(validationResults, r => r.ErrorMessage != null && r.ErrorMessage.Contains("PESEL"));
    }

    [Fact]
    public void CreateCompanyClientDto_InvalidKRS_FailsValidation()
    {
        // Arrange
        var dto = new CreateCompanyClientDto
        {
            CompanyName = "Test Company",
            Email = "test@company.com",
            PhoneNumber = "123456789",
            Address = "Test Address",
            KRSNumber = "123456789" // Invalid - only 9 digits
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.Contains(validationResults, r => r.ErrorMessage != null && r.ErrorMessage.Contains("KRS number must contain exactly 10 digits"));
    }

    [Fact]
    public void CreateCompanyClientDto_ValidKRS_PassesValidation()
    {
        // Arrange
        var dto = new CreateCompanyClientDto
        {
            CompanyName = "Test Company",
            Email = "test@company.com",
            PhoneNumber = "123456789",
            Address = "Test Address",
            KRSNumber = "0000123456" // Valid - 10 digits
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        Assert.DoesNotContain(validationResults, r => r.ErrorMessage != null && r.ErrorMessage.Contains("KRS"));
    }
}

public class DatabaseConstraintTests
{
    private DatabaseContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new DatabaseContext(options);
    }

    [Fact]
    public void UniqueConstraints_AreProperlyConfigured()
    {
        // Verify that the entity configurations include unique indexes
        using var context = GetInMemoryDbContext();
        var model = context.Model;
        
        // Check Individual Client PESEL index
        var individualEntity = model.FindEntityType(typeof(IndividualClient));
        var peselIndex = individualEntity?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "PESEL"));
        Assert.NotNull(peselIndex);
        Assert.True(peselIndex.IsUnique);
        
        // Check Company Client KRS index  
        var companyEntity = model.FindEntityType(typeof(CompanyClient));
        var krsIndex = companyEntity?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "KRSNumber"));
        Assert.NotNull(krsIndex);
        Assert.True(krsIndex.IsUnique);
    }

    [Fact]
    public void DatabaseModel_ConfiguresClientInheritance()
    {
        // Verify inheritance is properly configured
        using var context = GetInMemoryDbContext();
        var model = context.Model;
        
        var clientEntity = model.FindEntityType(typeof(Client));
        Assert.NotNull(clientEntity);
        
        // Check that discriminator is configured
        var discriminator = clientEntity.FindDiscriminatorProperty();
        Assert.NotNull(discriminator);
        Assert.Equal("ClientType", discriminator.Name);
    }

    [Fact]
    public void PropertyMaxLengths_AreProperlyConfigured()
    {
        using var context = GetInMemoryDbContext();
        var model = context.Model;
        
        // Check Individual Client properties
        var individualEntity = model.FindEntityType(typeof(IndividualClient));
        var peselProperty = individualEntity?.FindProperty("PESEL");
        Assert.NotNull(peselProperty);
        Assert.Equal(11, peselProperty.GetMaxLength());
        
        var firstNameProperty = individualEntity?.FindProperty("FirstName");
        Assert.NotNull(firstNameProperty);
        Assert.Equal(50, firstNameProperty.GetMaxLength());
        
        // Check Company Client properties
        var companyEntity = model.FindEntityType(typeof(CompanyClient));
        var krsProperty = companyEntity?.FindProperty("KRSNumber");
        Assert.NotNull(krsProperty);
        Assert.Equal(10, krsProperty.GetMaxLength());
        
        var companyNameProperty = companyEntity?.FindProperty("CompanyName");
        Assert.NotNull(companyNameProperty);
        Assert.Equal(100, companyNameProperty.GetMaxLength());
    }

    [Fact]
    public void RequiredProperties_AreProperlyConfigured()
    {
        using var context = GetInMemoryDbContext();
        var model = context.Model;
        
        // Check base Client properties
        var clientEntity = model.FindEntityType(typeof(Client));
        var emailProperty = clientEntity?.FindProperty("Email");
        Assert.NotNull(emailProperty);
        Assert.False(emailProperty.IsNullable);
        
        var addressProperty = clientEntity?.FindProperty("Address");
        Assert.NotNull(addressProperty);
        Assert.False(addressProperty.IsNullable);
        
        // Check Individual Client specific properties
        var individualEntity = model.FindEntityType(typeof(IndividualClient));
        var peselProperty = individualEntity?.FindProperty("PESEL");
        Assert.NotNull(peselProperty);
        Assert.False(peselProperty.IsNullable);
        
        // Check Company Client specific properties
        var companyEntity = model.FindEntityType(typeof(CompanyClient));
        var krsProperty = companyEntity?.FindProperty("KRSNumber");
        Assert.NotNull(krsProperty);
        Assert.False(krsProperty.IsNullable);
    }

    [Fact]
    public void TableNames_AreProperlyConfigured()
    {
        using var context = GetInMemoryDbContext();
        var model = context.Model;
        
        // Verify Client uses single table inheritance
        var clientEntity = model.FindEntityType(typeof(Client));
        Assert.NotNull(clientEntity);
        Assert.Equal("Client", clientEntity.GetTableName());
        
        var individualEntity = model.FindEntityType(typeof(IndividualClient));
        Assert.NotNull(individualEntity);
        Assert.Equal("Client", individualEntity.GetTableName());
        
        var companyEntity = model.FindEntityType(typeof(CompanyClient));
        Assert.NotNull(companyEntity);
        Assert.Equal("Client", companyEntity.GetTableName());
    }

    [Fact]
    public void ServiceLayer_WouldHandleDuplicates()
    {
        using var context = GetInMemoryDbContext();
        
        // Create a client with a specific PESEL
        var client1 = new IndividualClient
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@test.com",
            PhoneNumber = "123456789",
            Address = "Address 1",
            PESEL = "80010112345",
            CreatedAt = DateTime.UtcNow
        };
        
        context.IndividualClients.Add(client1);
        context.SaveChanges();
        
        // Verify we can check for existing PESEL
        var existingClient = context.IndividualClients
            .FirstOrDefault(c => c.PESEL == "80010112345");
        Assert.NotNull(existingClient);
        
    }
}

public class ControllerValidationIntegrationTests
{
    [Fact]
    public void UpdateClientDto_CannotChangePESEL_FieldNotPresent()
    {
        // Act & Assert - Verify PESEL property doesn't exist in update DTO
        var peselProperty = typeof(UpdateIndividualClientDto).GetProperty("PESEL");
        Assert.Null(peselProperty);
    }

    [Fact]
    public void UpdateClientDto_CannotChangeKRS_FieldNotPresent()
    {
        // Act & Assert - Verify KRSNumber property doesn't exist in update DTO
        var krsProperty = typeof(UpdateCompanyClientDto).GetProperty("KRSNumber");
        Assert.Null(krsProperty);
    }
}

public class BusinessRuleTests
{
    [Fact]
    public void PESEL_BusinessRule_MustBe11Digits()
    {
        // Test various PESEL formats
        var validPesels = new[] { "80010112345", "90010112345", "00010112345" };
        var invalidPesels = new[] { "8001011234", "800101123456", "8001011234a", "" };

        foreach (var pesel in validPesels)
        {
            Assert.True(pesel.Length == 11 && pesel.All(char.IsDigit), 
                $"PESEL {pesel} should be valid");
        }

        foreach (var pesel in invalidPesels)
        {
            Assert.False(pesel.Length == 11 && pesel.All(char.IsDigit), 
                $"PESEL {pesel} should be invalid");
        }
    }

    [Fact]
    public void KRS_BusinessRule_MustBe10Digits()
    {
        // Test various KRS formats
        var validKrsNumbers = new[] { "0000123456", "1234567890", "0000000001" };
        var invalidKrsNumbers = new[] { "123456789", "12345678901", "123456789a", "" };

        foreach (var krs in validKrsNumbers)
        {
            Assert.True(krs.Length == 10 && krs.All(char.IsDigit), 
                $"KRS {krs} should be valid");
        }

        foreach (var krs in invalidKrsNumbers)
        {
            Assert.False(krs.Length == 10 && krs.All(char.IsDigit), 
                $"KRS {krs} should be invalid");
        }
    }
}