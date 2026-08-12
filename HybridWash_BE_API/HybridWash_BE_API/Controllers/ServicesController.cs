
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HybridWash_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        public ServicesController(IServiceService serviceService) 
        { 
            _serviceService = serviceService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetActiveServices()
        {
            return Ok(await _serviceService.GetActiveServicesAsync());
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            var service = await _serviceService.GetServiceByIdAsync(id);
            return service == null || service.IsActive != true
                ? NotFound(new { Message = "Service not found." })
                : Ok(service);
        }
    }
}
