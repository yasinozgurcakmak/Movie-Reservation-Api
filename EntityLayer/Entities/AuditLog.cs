namespace EntityLayer.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public int? UserId { get; set; }
    public string EntityName { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string Changes { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
