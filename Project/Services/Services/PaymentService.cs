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
        var contract = await _context.Contracts
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Id == dto.ContractId);

        if (contract == null) throw new ArgumentException("Contract not found");
        if (contract.IsCancelled) throw new InvalidOperationException("Cannot pay for cancelled contract");
        if (DateTime.UtcNow > contract.EndDate) throw new InvalidOperationException("Payment deadline exceeded");

        var totalPaid = contract.Payments.Sum(p => p.Amount);
        var remainingAmount = contract.Price - totalPaid;

        if (dto.Amount > remainingAmount)
            throw new InvalidOperationException("Payment amount exceeds remaining balance");

        var payment = new Payment
        {
            ContractId = dto.ContractId,
            Amount = dto.Amount,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = dto.PaymentMethod
        };

        _context.Payments.Add(payment);

        // Check if contract is now fully paid
        if (totalPaid + dto.Amount >= contract.Price)
        {
            contract.IsSigned = true;
        }

        await _context.SaveChangesAsync();

        return new PaymentResponseDto
        {
            Id = payment.Id,
            ContractId = payment.ContractId,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod
        };
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
}