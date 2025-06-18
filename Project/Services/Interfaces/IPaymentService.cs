using Project.DTOs.PaymentDTOs;

namespace Project.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> CreatePayment(CreatePaymentDto dto);
    Task<List<PaymentResponseDto>> GetPaymentsForContract(int contractId);
}