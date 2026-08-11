using HybridWash.Services.DTOs.Loyalty;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HybridWash_BE_API.Controllers;

[Route("api/loyalty")]
[ApiController]
[Authorize(Roles = "Customer")]
public class LoyaltyController : ControllerBase
{
    private readonly ILoyaltyService _loyaltyService;

    public LoyaltyController(ILoyaltyService loyaltyService)
    {
        _loyaltyService = loyaltyService;
    }

    [HttpGet("me/summary")]
    public async Task<ActionResult<LoyaltySummaryDTO>> GetMySummary()
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized(new { Message = "Customer identity is missing from the token." });
        }

        var summary = await _loyaltyService.GetSummaryAsync(customerId);
        if (summary == null)
        {
            return NotFound(new { Message = "Customer not found." });
        }

        return Ok(summary);
    }

    [HttpGet("me/transactions")]
    public async Task<ActionResult<PointTransactionPageDTO>> GetMyPointTransactions(
        [FromQuery, Range(1, 1_000_000)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized(new { Message = "Customer identity is missing from the token." });
        }

        var result = await _loyaltyService.GetPointTransactionsAsync(customerId, page, pageSize);
        return Ok(result);
    }

    private bool TryGetCustomerId(out int customerId)
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return int.TryParse(subject, out customerId);
    }
}
