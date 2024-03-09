namespace TaskManager.Domain.Entities;

public class Board
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public required User User { get; set; }
    public Guid UserId { get; set; }
    
    public ICollection<Section>? Sections { get; set; }
    public ICollection<Assignment>? Assignments { get; set; }
    public ICollection<Tag>? Tags { get; set; }
}
