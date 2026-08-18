using System.Security.Claims;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HybridWash_BE_API.Controllers;

[Route("api/IncidentReport/{reportId:int}/images")]
[ApiController]
[Authorize(Roles = "Customer,Admin,Staff")]
public class IncidentReportImagesController : ControllerBase
{
    private readonly IIncidentReportImageService _incidentReportImageService;

    public IncidentReportImagesController(
        IIncidentReportImageService incidentReportImageService)
    {
        _incidentReportImageService = incidentReportImageService;
    }

    [HttpGet("{imageNumber:int}")]
    [Produces("image/jpeg", "image/png", "image/webp", "application/octet-stream")]
    public async Task<IActionResult> GetImage(
        int reportId,
        int imageNumber,
        CancellationToken cancellationToken)
    {
        var requesterIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var requesterRole = User.FindFirstValue(ClaimTypes.Role);
        if (!int.TryParse(requesterIdValue, out var requesterId)
            || string.IsNullOrWhiteSpace(requesterRole))
        {
            return Unauthorized(new { Message = "Invalid authentication token." });
        }

        try
        {
            var image = await _incidentReportImageService.GetImageAsync(
                reportId,
                imageNumber,
                requesterId,
                requesterRole,
                cancellationToken);

            Response.Headers.CacheControl = "private, max-age=300";
            return File(image.Content, image.ContentType);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}
