using System.Security.Claims;
using MarbleCompanion.API.Services;
using MarbleCompanion.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarbleCompanion.API.Controllers;

[ApiController]
[Route("api/habits")]
[Authorize]
public class HabitsController : ControllerBase
{
    private readonly IHabitService _habitService;

    public HabitsController(IHabitService habitService)
    {
        _habitService = habitService;
    }

    [HttpGet("library")]
    [ProducesResponseType(typeof(List<HabitLibraryItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLibrary()
    {
        var library = await _habitService.GetLibraryAsync();
        return Ok(library);
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(List<ActiveHabitDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveHabits()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var habits = await _habitService.GetActiveHabitsAsync(userId);
        return Ok(habits);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ActiveHabitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateHabit([FromBody] CreateHabitDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            var habit = await _habitService.CreateHabitAsync(userId, dto);
            return Ok(habit);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ActiveHabitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateHabit(Guid id, [FromBody] CreateHabitDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            var habit = await _habitService.UpdateHabitAsync(userId, id, dto);
            return Ok(habit);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHabit(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _habitService.DeleteHabitAsync(userId, id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/checkin")]
    [ProducesResponseType(typeof(HabitCheckinDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Checkin(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            var checkin = await _habitService.CheckinAsync(userId, id);
            return Ok(checkin);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
