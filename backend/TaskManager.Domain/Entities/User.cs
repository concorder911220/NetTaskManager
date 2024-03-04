namespace TaskManager.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Sub { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Picture { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiryTime { get; set; } 
}