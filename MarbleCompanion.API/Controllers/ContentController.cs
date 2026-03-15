using System.Security.Claims;
using MarbleCompanion.API.Services;
using MarbleCompanion.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarbleCompanion.API.Controllers;

[ApiController]
[Route("api/content")]
[Authorize]
public class ContentController : ControllerBase
{
    private readonly IContentService _contentService;

    public ContentController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ContentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var content = await _contentService.GetAllAsync();
        return Ok(content);
    }

    [HttpGet("today")]
    [ProducesResponseType(typeof(ContentSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetToday()
    {
        var content = await _contentService.GetTodaysFactAsync();
        return Ok(content);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            var content = await _contentService.GetByIdAsync(id, userId);
            return Ok(content);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/bookmark")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Bookmark(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _contentService.BookmarkAsync(userId, id);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ContentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateContentDto dto)
    {
        var content = await _contentService.CreateAsync(dto);
        return Ok(content);
    }
}
