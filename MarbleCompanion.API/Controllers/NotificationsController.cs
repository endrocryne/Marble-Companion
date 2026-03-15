using System.Security.Claims;
using MarbleCompanion.API.Services;
using MarbleCompanion.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarbleCompanion.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPut("preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePreferences([FromBody] NotificationPreferencesDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _notificationService.UpdatePreferencesAsync(userId, dto);
        return Ok();
    }

    [HttpPost("register-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterToken([FromBody] RegisterDeviceTokenDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _notificationService.RegisterTokenAsync(userId, dto);
        return Ok();
    }
}
