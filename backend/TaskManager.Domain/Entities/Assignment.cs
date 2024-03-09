namespace TaskManager.Domain.Entities;

public class Assignment
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? Deadline { get; set; }
    
    public ICollection<Tag>? Tags { get; set; }
    
    public required Board Board { get; set; }
    public Guid BoardId { get; set; }

    public required User CreatedBy { get; set; }
    public Guid CreatedById { get; set; }

    public required Section Section { get; set; }
    public Guid SectionId { get; set; }
}