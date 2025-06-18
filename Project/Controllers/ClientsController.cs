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
        if (client == null) return NotFound();
        return Ok(client);
    }

    [HttpPost("individual")]
    public async Task<IActionResult> CreateIndividual(CreateIndividualClientDto dto)
    {
        var client = await _clientService.CreateIndividualClient(dto);
        return Ok(client);
    }

    [HttpPost("company")]
    public async Task<IActionResult> CreateCompany(CreateCompanyClientDto dto)
    {
        var client = await _clientService.CreateCompanyClient(dto);
        return Ok(client);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _clientService.DeleteClient(id);
        if (!result) return NotFound();
        return NoContent();
    }
}