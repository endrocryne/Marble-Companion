using System.Security.Claims;
using MarbleCompanion.API.Services;
using MarbleCompanion.Shared.DTOs;
using MarbleCompanion.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarbleCompanion.API.Controllers;

[ApiController]
[Route("api/actions")]
[Authorize]
public class ActionsController : ControllerBase
{
    private readonly IActionService _actionService;

    public ActionsController(IActionService actionService)
    {
        _actionService = actionService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(LogActionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogAction([FromBody] LogActionRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            var response = await _actionService.LogActionAsync(userId, request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CarbonActionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActions(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] ActionCategory? category)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var actions = await _actionService.GetActionsAsync(userId, from, to, category);
        return Ok(actions);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAction(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _actionService.DeleteActionAsync(userId, id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ActionSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var summary = await _actionService.GetSummaryAsync(userId);
        return Ok(summary);
    }
}
