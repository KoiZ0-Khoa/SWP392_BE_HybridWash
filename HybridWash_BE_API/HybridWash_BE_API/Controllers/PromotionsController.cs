using HybridWash.Services.DTOs.Promotion;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HybridWash_BE_API.Controllers;

[Route("api/promotions")]
[ApiController]
[Produces("application/json")]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionsController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [AllowAnonymous]
    [HttpGet("public")]
    public async Task<ActionResult<IReadOnlyList<PromotionDTO>>> GetPublicPromotions()
    {
        return Ok(await _promotionService.GetPublicAsync());
    }
}
