using LiteDB;
using MarbleCompanion.Shared.Enums;

namespace MarbleCompanion.Mobile.Models;

public class LocalAction
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    public string? ServerId { get; set; }

    public ActionCategory Category { get; set; }

    public string? TemplateId { get; set; }

    public Dictionary<string, string> DetailedData { get; set; } = new();

    public double Co2ESaved { get; set; }

    public int LpAwarded { get; set; }

    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public bool Synced { get; set; }
}
