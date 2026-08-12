using HybridWash.Services.DTOs.Service;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HybridWash_BE_API.Controllers;

[Route("api/admin/services")]
[ApiController]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminServicesController : ControllerBase
{
    private readonly IServiceService _serviceService;

    public AdminServicesController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceDto>>> GetAll()
    {
        return Ok(await _serviceService.GetAllServicesAsync());
    }

    [HttpGet("{serviceId:int}")]
    public async Task<ActionResult<ServiceDto>> GetById(int serviceId)
    {
        var service = await _serviceService.GetServiceByIdAsync(serviceId);
        return service == null
            ? NotFound(new { Message = "Service not found." })
            : Ok(service);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceDto>> Create(UpsertServiceDto request)
    {
        try
        {
            var service = await _serviceService.CreateServiceAsync(request);
            return CreatedAtAction(nameof(GetById), new { serviceId = service.ServiceId }, service);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{serviceId:int}")]
    public async Task<IActionResult> Update(int serviceId, UpsertServiceDto request)
    {
        try
        {
            return await _serviceService.UpdateServiceAsync(serviceId, request)
                ? NoContent()
                : NotFound(new { Message = "Service not found." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPatch("{serviceId:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int serviceId)
    {
        return await _serviceService.DeactivateServiceAsync(serviceId)
            ? NoContent()
            : NotFound(new { Message = "Service not found." });
    }
}
