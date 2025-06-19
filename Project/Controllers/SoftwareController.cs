using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.DTOs.SoftwareDTOs;
using Project.Services.Interfaces;

namespace Project.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SoftwareController : ControllerBase
{
    private readonly ISoftwareService _softwareService;
    private readonly ILogger<SoftwareController> _logger;

    public SoftwareController(ISoftwareService softwareService, ILogger<SoftwareController> logger)
    {
        _softwareService = softwareService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var software = await _softwareService.GetAllSoftware();
            return Ok(software);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all software");
            return StatusCode(500, "An error occurred while retrieving software");
        }
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var software = await _softwareService.GetSoftwareById(id);
            if (software == null) 
                return NotFound($"Software with ID {id} not found");
            
            return Ok(software);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving software with ID {SoftwareId}", id);
            return StatusCode(500, "An error occurred while retrieving software");
        }
    }
    
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(string category)
    {
        try
        {
            var software = await _softwareService.GetSoftwareByCategory(category);
            return Ok(software);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving software for category {Category}", category);
            return StatusCode(500, "An error occurred while retrieving software");
        }
    }
    
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateSoftwareDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var software = await _softwareService.CreateSoftware(dto);
            return CreatedAtAction(nameof(GetById), new { id = software.Id }, software);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating software {SoftwareName}", dto.Name);
            return StatusCode(500, "An error occurred while creating software");
        }
    }
    
    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSoftwareDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var software = await _softwareService.UpdateSoftware(id, dto);
            if (software == null) 
                return NotFound($"Software with ID {id} not found");
            
            return Ok(software);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating software with ID {SoftwareId}", id);
            return StatusCode(500, "An error occurred while updating software");
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _softwareService.DeleteSoftware(id);
            if (!result) 
                return NotFound($"Software with ID {id} not found");
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting software with ID {SoftwareId}", id);
            return StatusCode(500, "An error occurred while deleting software");
        }
    }
}