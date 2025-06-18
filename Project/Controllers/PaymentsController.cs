using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.DTOs.PaymentDTOs;
using Project.Services.Interfaces;

namespace Project.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePaymentDto dto)
    {
        var payment = await _paymentService.CreatePayment(dto);
        return Ok(payment);
    }

    [HttpGet("contract/{contractId}")]
    public async Task<IActionResult> GetForContract(int contractId)
    {
        var payments = await _paymentService.GetPaymentsForContract(contractId);
        return Ok(payments);
    }
}