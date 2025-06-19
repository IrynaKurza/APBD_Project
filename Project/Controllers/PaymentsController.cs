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
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var payment = await _paymentService.CreatePayment(dto);
            return CreatedAtAction(nameof(GetForContract), new { contractId = dto.ContractId }, payment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while processing payment");
        }
    }

    [HttpGet("contract/{contractId}")]
    public async Task<IActionResult> GetForContract(int contractId)
    {
        var payments = await _paymentService.GetPaymentsForContract(contractId);
        return Ok(payments);
    }
    
    [HttpPost("validate")]
    public async Task<IActionResult> ValidatePayment([FromQuery] int contractId, [FromQuery] decimal amount)
    {
        try
        {
            var validation = await _paymentService.ValidatePayment(contractId, amount);
            return Ok(validation);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while validating payment");
        }
    }
    
}