using ErrorOr;
using Mapster;
using MediatR;
using Microsoft.Extensions.Options;
using TaskManager.Common;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Services;

namespace TaskManager.Application.Commands.Login;

public class LoginUserRequest : IRequest<LoginUserResponse>
{
    public required string Sub { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Picture { get; set; }
}

public class LoginUserResponse
{
    public required Guid Id { get; set; }
    public required string Sub { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Picture { get; set; }
    public required string RefreshToken { get; set; }
}

public class LoginUserRequestHandler(AppDbContext appDbContext, IOptions<JwtOptions> jwtOptions, JwtService jwtService)
    : IRequestHandler<LoginUserRequest, LoginUserResponse>
{
    private readonly AppDbContext _appDbContext = appDbContext;
    private readonly IOptions<JwtOptions> _jwtOptions = jwtOptions;
    private readonly JwtService _jwtService = jwtService;

    public async Task<LoginUserResponse> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var user = _appDbContext.Users.FirstOrDefault(u => u.Sub == request.Sub);

        if (user is null)
        {
            user = request.Adapt<User>();
            user.Id = Guid.NewGuid();
            await _appDbContext.Users.AddAsync(user, cancellationToken);
        }
        else
        {
            user.Username = request.Username;
            user.Email = request.Email;
            user.Picture = request.Picture;
        }
        
        user.RefreshToken = jwtService.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtOptions.Value.RefreshTokenExpiryTimeInDays);
        
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return user.Adapt<LoginUserResponse>();
    }
}
