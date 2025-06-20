using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.DTOs.PaymentDTOs;
using Project.Models;
using Project.Services.Interfaces;

namespace Project.Services.Services;

public class PaymentService : IPaymentService
{
    private readonly DatabaseContext _context;

    public PaymentService(DatabaseContext context)
    {
        _context = context;
    }
    
    public async Task<PaymentResponseDto> CreatePayment(CreatePaymentDto dto)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 1. Find contract with all related data
        var contract = await _context.Contracts
            .Include(c => c.Payments)
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == dto.ContractId);

        if (contract == null) 
            throw new ArgumentException("Contract not found");

        // 2. Validate contract state
        if (contract.IsCancelled) 
            throw new InvalidOperationException("Cannot pay for cancelled contract");

        if (contract.IsSigned)
            throw new InvalidOperationException("Contract is already fully paid");

        // 3. Check payment deadline
        if (DateTime.UtcNow > contract.EndDate)
        {
            // Auto-cancel expired contract
            contract.IsCancelled = true;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            throw new InvalidOperationException("Payment deadline exceeded - contract has been cancelled");
        }

        // 4. Validate payment amount
        if (dto.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero");

        var totalPaid = contract.Payments.Sum(p => p.Amount);
        var remainingAmount = contract.Price - totalPaid;

        if (dto.Amount > remainingAmount)
            throw new InvalidOperationException($"Payment amount ({dto.Amount:C}) exceeds remaining balance ({remainingAmount:C})");

        // 5. Validate payment method
        var allowedPaymentMethods = new[] { "Credit Card", "Bank Transfer", "Cash", "Check", "Wire Transfer" };
        if (!allowedPaymentMethods.Contains(dto.PaymentMethod))
            throw new ArgumentException($"Invalid payment method. Allowed methods: {string.Join(", ", allowedPaymentMethods)}");

        // 6. Create payment
        var payment = new Payment
        {
            ContractId = dto.ContractId,
            Amount = dto.Amount,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = dto.PaymentMethod
        };

        _context.Payments.Add(payment);

        // 7. Check if contract is now fully paid
        var newTotalPaid = totalPaid + dto.Amount;
        if (Math.Abs(newTotalPaid - contract.Price) < 0.01m) // Account for floating point precision
        {
            contract.IsSigned = true;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new PaymentResponseDto
        {
            Id = payment.Id,
            ContractId = payment.ContractId,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            ContractFullyPaid = contract.IsSigned,
            RemainingBalance = contract.Price - newTotalPaid
        };
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}

    public async Task<List<PaymentResponseDto>> GetPaymentsForContract(int contractId)
    {
        var payments = await _context.Payments
            .Where(p => p.ContractId == contractId)
            .Select(p => new PaymentResponseDto
            {
                Id = p.Id,
                ContractId = p.ContractId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod
            }).ToListAsync();

        return payments;
    }
    
    
    public async Task<PaymentValidationDto> ValidatePayment(int contractId, decimal amount)
    {
        var contract = await _context.Contracts
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Id == contractId);

        if (contract == null)
            return new PaymentValidationDto { IsValid = false, ErrorMessage = "Contract not found" };

        if (contract.IsCancelled)
            return new PaymentValidationDto { IsValid = false, ErrorMessage = "Contract is cancelled" };

        if (contract.IsSigned)
            return new PaymentValidationDto { IsValid = false, ErrorMessage = "Contract is already fully paid" };

        if (DateTime.UtcNow > contract.EndDate)
            return new PaymentValidationDto { IsValid = false, ErrorMessage = "Payment deadline has passed" };

        var totalPaid = contract.Payments.Sum(p => p.Amount);
        var remainingAmount = contract.Price - totalPaid;

        if (amount > remainingAmount)
            return new PaymentValidationDto 
            { 
                IsValid = false, 
                ErrorMessage = $"Amount exceeds remaining balance of {remainingAmount:C}" 
            };

        if (amount <= 0)
            return new PaymentValidationDto { IsValid = false, ErrorMessage = "Amount must be greater than zero" };

        return new PaymentValidationDto 
        { 
            IsValid = true, 
            RemainingBalance = remainingAmount,
            WillCompleteContract = Math.Abs(totalPaid + amount - contract.Price) < 0.01m
        };
    }
    
}