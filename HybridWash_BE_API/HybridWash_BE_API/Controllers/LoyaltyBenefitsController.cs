using HybridWash.Services.DTOs.Promotion;
using HybridWash.Services.DTOs.Reward;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HybridWash_BE_API.Controllers;

[Route("api/loyalty/me")]
[ApiController]
[Authorize(Roles = "Customer")]
[Produces("application/json")]
public class LoyaltyBenefitsController : ControllerBase
{
    private readonly IPromotionService _promotionService;
    private readonly IRewardService _rewardService;

    public LoyaltyBenefitsController(
        IPromotionService promotionService,
        IRewardService rewardService)
    {
        _promotionService = promotionService;
        _rewardService = rewardService;
    }

    [HttpGet("promotions")]
    public async Task<ActionResult<IReadOnlyList<PromotionDTO>>> GetEligiblePromotions()
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized(new { Message = "Customer identity is missing from the token." });
        }

        try
        {
            return Ok(await _promotionService.GetEligibleAsync(customerId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpGet("rewards")]
    public async Task<ActionResult<IReadOnlyList<RewardDTO>>> GetEligibleRewards()
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized(new { Message = "Customer identity is missing from the token." });
        }

        try
        {
            return Ok(await _rewardService.GetEligibleAsync(customerId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost("rewards/{rewardId:int}/redeem")]
    public async Task<ActionResult<RewardRedemptionDTO>> RedeemReward(
        int rewardId,
        RedeemRewardRequestDTO request)
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized(new { Message = "Customer identity is missing from the token." });
        }

        try
        {
            return Ok(await _rewardService.RedeemAsync(customerId, rewardId, request.RequestId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("redemptions")]
    public async Task<ActionResult<IReadOnlyList<RewardRedemptionDTO>>> GetRedemptions()
    {
        if (!TryGetCustomerId(out var customerId))
        {
            return Unauthorized(new { Message = "Customer identity is missing from the token." });
        }

        try
        {
            return Ok(await _rewardService.GetRedemptionsAsync(customerId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    private bool TryGetCustomerId(out int customerId)
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return int.TryParse(subject, out customerId);
    }
}
