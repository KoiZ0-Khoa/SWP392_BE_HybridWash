using HybridWash.Services.DTOs.Tier;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HybridWash_BE_API.Controllers;

[Route("api/admin/tier-rules")]
[ApiController]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminTierRulesController : ControllerBase
{
    private readonly ITierService _tierService;

    public AdminTierRulesController(ITierService tierService)
    {
        _tierService = tierService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TierRuleDTO>>> GetAll()
    {
        return Ok(await _tierService.GetRulesAsync());
    }

    [HttpPut("{tierName}")]
    public async Task<ActionResult<TierRuleDTO>> Update(
        string tierName,
        UpdateTierRuleDTO request)
    {
        try
        {
            var result = await _tierService.UpdateRuleAsync(tierName, request);
            return result == null
                ? NotFound(new { Message = "Tier rule not found." })
                : Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
    }

    [HttpPost("review")]
    public async Task<ActionResult<TierReviewResultDTO>> RunMonthlyReview()
    {
        return Ok(await _tierService.RunMonthlyReviewAsync(
            DateTime.UtcNow,
            onlyDueCustomers: false));
    }
}
