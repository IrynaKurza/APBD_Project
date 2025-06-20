using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.DTOs.ClientDTOs;
using Project.Services.Interfaces;

namespace Project.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var clients = await _clientService.GetClients();
        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var client = await _clientService.GetClientById(id);
        return Ok(client);
    }

    [HttpPost("individual")]
    public async Task<IActionResult> CreateIndividual(CreateIndividualClientDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = await _clientService.CreateIndividualClient(dto);
            return Created($"/api/Clients/{client.Id}", client); 
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while creating the client");
        }
    }

    [HttpPost("company")]
    public async Task<IActionResult> CreateCompany(CreateCompanyClientDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = await _clientService.CreateCompanyClient(dto);
            return Created($"/api/Clients/{client.Id}", client); 
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while creating the client");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _clientService.DeleteClient(id);
            if (!result) return NotFound($"Client with ID {id} not found");
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPut("individual/{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateIndividual(int id, [FromBody] UpdateIndividualClientDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = await _clientService.UpdateIndividualClient(id, dto);
            if (client == null) 
                return NotFound($"Individual client with ID {id} not found");
        
            return Ok(client);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while updating the client");
        }
    }
    
    [HttpPut("company/{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateCompany(int id, [FromBody] UpdateCompanyClientDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var client = await _clientService.UpdateCompanyClient(id, dto);
            if (client == null) 
                return NotFound($"Company client with ID {id} not found");
        
            return Ok(client);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while updating the client");
        }
    }
    
    [HttpGet("{id:int}/returning-status")]
    public async Task<IActionResult> GetReturningStatus(int id)
    {
        try
        {
            var isReturning = await _clientService.IsReturningClient(id);
            return Ok(new { ClientId = id, IsReturningClient = isReturning });
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while checking client status");
        }
    }
    
}