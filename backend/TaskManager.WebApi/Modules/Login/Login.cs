using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Commands.Login;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Services;
using TaskManager.WebApi.Extensions;

namespace TaskManager.WebApi.Modules.Login;

public class Login : IModule
{
    private const string Callback = "https://localhost:7240/api/auth/google/callback";

    public IResult GoogleLoginHandler(ExternalAuthService externalAuthService)
    {
        var uri = externalAuthService.GetGoogleAuthUrl(Callback);
        return Results.Redirect(uri);
    }

    public async Task<IResult> GoogleCallbackHandler(
        ExternalAuthService externalAuthService, 
        JwtService jwtService,
        ISender sender,
        [FromQuery] string code)
    {
        var tokens = await externalAuthService.GetCredentials(code, Callback);
        var userInfo = await externalAuthService.GetUserInfo(tokens.AccessToken);

        var user = new User
        {
            Email = userInfo.Email,
            Picture = userInfo.Picture,
            Sub = userInfo.Id,
            Username = userInfo.Name
        };

        var userResponse = await sender.Send(user.Adapt<LoginUserRequest>());
        
        var token = jwtService.GenerateToken(user);
        
        return Results.Ok(new
        {
            access_token = token,
            user_info = userResponse
        });
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("auth");

        group.MapGet("google", GoogleLoginHandler);
        group.MapGet("google/callback", GoogleCallbackHandler);

        return group;
    }
}