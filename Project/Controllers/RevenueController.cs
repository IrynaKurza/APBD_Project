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
    
    // Calculate revenue for the company
    [HttpGet]
    public async Task<IActionResult> GetRevenue([FromQuery] RevenueQueryDto query)
    {
        try
        {
            // Validate revenue type
            if (!string.IsNullOrEmpty(query.RevenueType) && 
                query.RevenueType.ToLower() != "current" && 
                query.RevenueType.ToLower() != "predicted")
            {
                return BadRequest("RevenueType must be 'Current' or 'Predicted'");
            }

            var revenue = await _revenueService.CalculateRevenue(query);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while calculating revenue", error = ex.Message });
        }
    }
    
    // Get current revenue (signed contracts only)
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentRevenue([FromQuery] int? softwareId, [FromQuery] string currency = "PLN")
    {
        try
        {
            var query = new RevenueQueryDto 
            { 
                SoftwareId = softwareId, 
                Currency = currency, 
                RevenueType = "Current" 
            };
            
            var revenue = await _revenueService.CalculateRevenue(query);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while calculating current revenue", error = ex.Message });
        }
    }
    
    // Get predicted revenue (signed + unsigned contracts)
    [HttpGet("predicted")]
    public async Task<IActionResult> GetPredictedRevenue([FromQuery] int? softwareId, [FromQuery] string currency = "PLN")
    {
        try
        {
            var query = new RevenueQueryDto 
            { 
                SoftwareId = softwareId, 
                Currency = currency, 
                RevenueType = "Predicted" 
            };
            
            var revenue = await _revenueService.CalculateRevenue(query);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while calculating predicted revenue", error = ex.Message });
        }
    }
}