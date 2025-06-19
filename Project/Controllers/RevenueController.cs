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
    
    [HttpGet]
    public async Task<IActionResult> GetRevenue([FromQuery] RevenueQueryDto query)
    {
        try
        {
            var revenue = await _revenueService.CalculateRevenue(query);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while calculating revenue", error = ex.Message });
        }
    }
}