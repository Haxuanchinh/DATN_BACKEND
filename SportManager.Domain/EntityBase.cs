namespace SportManager.Domain;

public abstract class EntityBase : EntityBase<Guid>
{
    public EntityBase()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
}

public abstract class EntityBase<TKey>
{
    public TKey Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool? IsDeleted { get; set; } = false;
    public string? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}
