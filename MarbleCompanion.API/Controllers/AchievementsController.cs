using System.Security.Claims;
using MarbleCompanion.API.Services;
using MarbleCompanion.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarbleCompanion.API.Controllers;

[ApiController]
[Route("api/achievements")]
[Authorize]
public class AchievementsController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public AchievementsController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AchievementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var achievements = await _achievementService.GetAllAsync();
        return Ok(achievements);
    }

    [HttpGet("unlocked")]
    [ProducesResponseType(typeof(List<UserAchievementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnlocked()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var unlocked = await _achievementService.GetUnlockedAsync(userId);
        return Ok(unlocked);
    }
}
