using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.DTOs.RevenueDTOs;
using Project.Services.Interfaces;

namespace Project.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RevenueController : ControllerBase
{
    private readonly IRevenueService _revenueService;

    public RevenueController(IRevenueService revenueService)
    {
        _revenueService = revenueService;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentRevenue([FromQuery] RevenueQueryDto query)
    {
        var revenue = await _revenueService.CalculateCurrentRevenue(query);
        return Ok(revenue);
    }

    [HttpGet("predicted")]
    public async Task<IActionResult> GetPredictedRevenue([FromQuery] RevenueQueryDto query)
    {
        var revenue = await _revenueService.CalculatePredictedRevenue(query);
        return Ok(revenue);
    }
}