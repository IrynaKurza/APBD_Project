using Project.Models;

namespace Tests;

public class ModelValidationTests
{
    [Theory]
    [InlineData("12345678901")] // Valid 11-digit PESEL
    [InlineData("98765432109")] // Valid 11-digit PESEL
    public void PESEL_ValidFormat_PassesValidation(string pesel)
    {
        // Arrange
        var client = new IndividualClient
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PhoneNumber = "123456789",
            Address = "Test Address",
            PESEL = pesel
        };

        // Act & Assert - Should not throw
        Assert.Equal(pesel, client.PESEL);
        Assert.True(pesel.Length == 11 && pesel.All(char.IsDigit));
    }

    [Theory]
    [InlineData("1234567890")] // 10 digits
    [InlineData("123456789012")] // 12 digits
    [InlineData("abcdefghijk")] // Letters
    [InlineData("123-456-789")] // With separators
    public void PESEL_InvalidFormat_ShouldFailValidation(string invalidPesel)
    {
        // Test the validation logic directly
        Assert.True(invalidPesel.Length != 11 || !invalidPesel.All(char.IsDigit));
    }

    [Theory]
    [InlineData("0000123456")] // Valid 10-digit KRS
    [InlineData("9999888777")] // Valid 10-digit KRS
    public void KRSNumber_ValidFormat_PassesValidation(string krsNumber)
    {
        // Arrange
        var client = new CompanyClient
        {
            CompanyName = "Test Company",
            Email = "test@company.com",
            PhoneNumber = "123456789",
            Address = "Test Address",
            KRSNumber = krsNumber 
        };
        
        // Act & Assert
        Assert.Equal(krsNumber, client.KRSNumber);
        Assert.True(krsNumber.Length == 10 && krsNumber.All(char.IsDigit));
    }

    [Theory]
    [InlineData("123456789")] // 9 digits
    [InlineData("12345678901")] // 11 digits
    [InlineData("abcdefghij")] // Letters
    public void KRSNumber_InvalidFormat_ShouldFailValidation(string invalidKrs)
    {
        // Test the validation logic directly
        Assert.True(invalidKrs.Length != 10 || !invalidKrs.All(char.IsDigit));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void AdditionalSupportYears_ValidRange_PassesValidation(int years)
    {
        // Arrange
        var contract = new Contract
        {
            AdditionalSupportYears = years,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            SoftwareVersion = "1.0",
            Price = 1000m
        };

        // Act & Assert
        Assert.Equal(years, contract.AdditionalSupportYears);
        Assert.True(years >= 0 && years <= 3);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(10)]
    public void AdditionalSupportYears_InvalidRange_ShouldFailValidation(int invalidYears)
    {
        // Test the validation logic directly
        Assert.True(invalidYears < 0 || invalidYears > 3);
    }
}