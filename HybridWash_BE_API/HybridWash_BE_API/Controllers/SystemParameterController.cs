using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HybridWash.Entities.DTOs;
using HybridWash.Services.Interfaces;

namespace HybridWash_BE_API.Controllers
{
    [Route("api/system-parameters")]
    [ApiController]
    public class SystemParameterController : ControllerBase
    {
        private readonly ISystemParameterService _systemParameterService;

        public SystemParameterController(ISystemParameterService systemParameterService)
        {
            _systemParameterService = systemParameterService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSystemParameter()
        {
            var parameter = await _systemParameterService.GetSystemParameterAsync();
            return Ok(parameter);
        }

        [HttpPut]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateSystemParameter([FromBody] SystemParameterUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedParameter = await _systemParameterService.UpdateSystemParameterAsync(updateDto);
            return Ok(updatedParameter);
        }
    }
}
