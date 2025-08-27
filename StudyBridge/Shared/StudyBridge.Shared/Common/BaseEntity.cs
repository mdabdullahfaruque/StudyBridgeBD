namespace StudyBridge.Shared.Common;

public abstract record BaseEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public abstract record BaseAuditableEntity : BaseEntity
{
    public string? CreatedBy { get; init; }
    public string? UpdatedBy { get; set; }
}
