namespace TaskManager.Domain.Entities;

public class Section
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string HexColor { get; set; }
    
    public required Board Board { get; set; }
    public Guid BoardId { get; set; }

    public ICollection<Assignment>? Assignments { get; set; }
}
