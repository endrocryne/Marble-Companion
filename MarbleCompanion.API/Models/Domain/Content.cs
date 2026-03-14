using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.API.Models.Domain;

public class Content
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string BodyMarkdown { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public int ReadingTimeMinutes { get; set; }
    public ContentDifficulty Difficulty { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
