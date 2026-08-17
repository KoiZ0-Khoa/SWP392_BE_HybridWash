using HybridWash.Services.DTOs.Loyalty;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HybridWash_BE_API.Controllers;

[Route("api/admin/loyalty")]
[ApiController]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminLoyaltyController : ControllerBase
{
    private readonly ILoyaltyService _loyaltyService;

    public AdminLoyaltyController(ILoyaltyService loyaltyService)
    {
        _loyaltyService = loyaltyService;
    }

    [HttpPost("expire-points")]
    public async Task<ActionResult<PointExpiryResultDTO>> ExpirePoints()
    {
        return Ok(await _loyaltyService.ExpirePointsAsync(DateTime.UtcNow));
    }
}
