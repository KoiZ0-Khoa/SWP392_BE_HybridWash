
using HybridWash.Services.Interfaces;
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

        [HttpGet]
        public async Task<IActionResult> GetActiveServices()
        {
            try
            {
                var services = await _serviceService.GetActiveServicesAsync();
                return Ok(services);

            }catch (Exception ex)
            {
                return BadRequest(new {message = ex.Message});
            }

        }

        [HttpGet("/id")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            try
            {
                var service = await _serviceService.GetServiceByIdAsync(id);
                return Ok(service);

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

    }
}
