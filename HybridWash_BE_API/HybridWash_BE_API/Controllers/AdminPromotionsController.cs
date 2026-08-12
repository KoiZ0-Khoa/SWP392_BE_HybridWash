using HybridWash.Services.DTOs.Promotion;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HybridWash_BE_API.Controllers;

[Route("api/admin/promotions")]
[ApiController]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminPromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public AdminPromotionsController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PromotionDTO>>> GetAll()
    {
        return Ok(await _promotionService.GetAllAsync());
    }

    [HttpGet("{promotionId:int}")]
    public async Task<ActionResult<PromotionDTO>> GetById(int promotionId)
    {
        var promotion = await _promotionService.GetByIdAsync(promotionId);
        return promotion == null
            ? NotFound(new { Message = "Promotion not found." })
            : Ok(promotion);
    }

    [HttpPost]
    public async Task<ActionResult<PromotionDTO>> Create(UpsertPromotionDTO request)
    {
        try
        {
            var promotion = await _promotionService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { promotionId = promotion.PromotionId }, promotion);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{promotionId:int}")]
    public async Task<IActionResult> Update(int promotionId, UpsertPromotionDTO request)
    {
        try
        {
            return await _promotionService.UpdateAsync(promotionId, request)
                ? NoContent()
                : NotFound(new { Message = "Promotion not found." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPatch("{promotionId:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int promotionId)
    {
        return await _promotionService.DeactivateAsync(promotionId)
            ? NoContent()
            : NotFound(new { Message = "Promotion not found." });
    }
}
