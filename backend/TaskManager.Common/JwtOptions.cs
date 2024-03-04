namespace TaskManager.Common;

public class JwtOptions
{
    public string Key { get; set; } = null!;
    public int LifetimeInMinutes { get; set; }
    public int RefreshTokenExpiryTimeInDays { get; set; }
}