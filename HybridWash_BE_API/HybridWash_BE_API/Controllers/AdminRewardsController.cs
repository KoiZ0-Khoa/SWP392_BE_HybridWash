using HybridWash.Services.DTOs.Reward;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HybridWash_BE_API.Controllers;

[Route("api/admin/rewards")]
[ApiController]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminRewardsController : ControllerBase
{
    private readonly IRewardService _rewardService;

    public AdminRewardsController(IRewardService rewardService)
    {
        _rewardService = rewardService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RewardDTO>>> GetAll()
    {
        return Ok(await _rewardService.GetAllAsync());
    }

    [HttpGet("{rewardId:int}")]
    public async Task<ActionResult<RewardDTO>> GetById(int rewardId)
    {
        var reward = await _rewardService.GetByIdAsync(rewardId);
        return reward == null
            ? NotFound(new { Message = "Reward not found." })
            : Ok(reward);
    }

    [HttpPost]
    public async Task<ActionResult<RewardDTO>> Create(UpsertRewardDTO request)
    {
        try
        {
            var reward = await _rewardService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { rewardId = reward.RewardId }, reward);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{rewardId:int}")]
    public async Task<IActionResult> Update(int rewardId, UpsertRewardDTO request)
    {
        try
        {
            return await _rewardService.UpdateAsync(rewardId, request)
                ? NoContent()
                : NotFound(new { Message = "Reward not found." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPatch("{rewardId:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int rewardId)
    {
        return await _rewardService.DeactivateAsync(rewardId)
            ? NoContent()
            : NotFound(new { Message = "Reward not found." });
    }
}
