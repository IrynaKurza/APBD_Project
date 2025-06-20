using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.DTOs.ContractDTOs;
using Project.Services.Interfaces;

namespace Project.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var contracts = await _contractService.GetContracts();
        return Ok(contracts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var contract = await _contractService.GetContract(id);
        return Ok(contract);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateContractDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var contract = await _contractService.CreateContract(dto);
        
            // Fix the nullable reference warning
            if (contract == null)
                return StatusCode(500, "Failed to create contract");

            return CreatedAtAction(nameof(Get), new { id = contract.Id }, contract);
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
            return StatusCode(500, "An error occurred while creating the contract");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _contractService.RemoveContract(id);
        if (!result) return NotFound();
        return NoContent();
    }
}