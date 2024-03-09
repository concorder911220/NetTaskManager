namespace TaskManager.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
    
    public required Board Board { get; set; }
    public Guid BoardId { get; set; }

    public ICollection<Assignment>? Assignments { get; set; }
}
