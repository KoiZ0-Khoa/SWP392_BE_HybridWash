using HybridWash.Services.DTOs.Tier;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HybridWash_BE_API.Controllers;

[Route("api/tiers")]
[ApiController]
[AllowAnonymous]
[Produces("application/json")]
public class TiersController : ControllerBase
{
    private readonly ITierService _tierService;

    public TiersController(ITierService tierService)
    {
        _tierService = tierService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PublicTierRuleDTO>>> GetAll()
    {
        return Ok(await _tierService.GetPublicRulesAsync());
    }
}
