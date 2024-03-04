using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ErrorOr;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Services;

public class JwtService(IOptions<JwtOptions> jwtOptions)
{
    private readonly IOptions<JwtOptions> _jwtOptions = jwtOptions;

    public string GenerateToken(User user)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Value.Key));
        var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("id", user.Id.ToString()),
            new("sub", user.Sub),
            new("email", user.Email),
            new("name", user.Username),
            new("picture", user.Username)
        };

        var tokenOptions = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.Value.LifetimeInMinutes),
            signingCredentials: signinCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    
    public ErrorOr<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Value.Key)),
            ValidateLifetime = false,
            ValidateAudience = false,
            ValidateIssuer = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
        {
            return Error.Failure("Invalid token");
        }

        return principal;
    }
}