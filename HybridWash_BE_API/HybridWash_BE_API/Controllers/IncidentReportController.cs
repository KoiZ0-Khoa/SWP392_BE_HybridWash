using HybridWash.Services.DTOs;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HybridWash_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentReportController : ControllerBase
    {
        private readonly IIncidentReportService _service;

        public IncidentReportController(IIncidentReportService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> CreateReport([FromForm] CreateIncidentReportDto request)
        {
            try
            {
                var customerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(customerIdStr) || !int.TryParse(customerIdStr, out int customerId))
                {
                    return Unauthorized(new { Message = "Customer ID kh�ng h?p l?." });
                }

                var result = await _service.CreateReportAsync(customerId, request);
                return Ok(new { Success = true, Message = "G?i b�o c�o th�nh c�ng.", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("my-reports")]
        public async Task<IActionResult> GetMyReports()
        {
            try
            {
                var customerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(customerIdStr) || !int.TryParse(customerIdStr, out int customerId))
                {
                    return Unauthorized(new { Message = "Customer ID kh�ng h?p l?." });
                }

                var result = await _service.GetMyReportsAsync(customerId);
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllReports()
        {
            try
            {
                var result = await _service.GetAllReportsAsync();
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPut("admin/{id}/resolve")]
        public async Task<IActionResult> ResolveReport(int id, [FromBody] ResolveIncidentReportDto request)
        {
            try
            {
                var result = await _service.ResolveReportAsync(id, request);
                return Ok(new { Success = true, Message = "C?p nh?t tr?ng th�i th�nh c�ng.", Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
