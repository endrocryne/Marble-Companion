using System.Security.Claims;
using MarbleCompanion.API.Services;
using MarbleCompanion.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarbleCompanion.API.Controllers;

[ApiController]
[Route("api/offsets")]
[Authorize]
public class OffsetsController : ControllerBase
{
    private readonly IOffsetService _offsetService;

    public OffsetsController(IOffsetService offsetService)
    {
        _offsetService = offsetService;
    }

    [HttpGet("credits")]
    [ProducesResponseType(typeof(List<OffsetCreditDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCredits()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var credits = await _offsetService.GetCreditsAsync(userId);
        return Ok(credits);
    }

    [HttpPost("redeem")]
    [ProducesResponseType(typeof(OffsetHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Redeem([FromBody] RedeemOffsetRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            var history = await _offsetService.RedeemAsync(userId, request);
            return Ok(history);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(List<OffsetHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var history = await _offsetService.GetHistoryAsync(userId);
        return Ok(history);
    }
}
