using ErrorOr;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using TaskManager.Application.Commands.Login;
using TaskManager.Common;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Services;
using TaskManager.WebApi.Extensions;

namespace TaskManager.WebApi.Modules.Auth;

public class AuthModule : IModule
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
        IOptions<JwtOptions> jwtOptions,
        ISender sender,
        HttpContext context,
        [FromQuery] string code)
    {
        var tokens = await externalAuthService.GetCredentials(code, Callback);

        if (tokens.IsError)
        {
            return CustomResults.ErrorJson(400, tokens.Errors);
        }
        
        var userInfo = await externalAuthService.GetUserInfo(tokens.Value.AccessToken);

        var user = new User
        {
            Email = userInfo.Email,
            Picture = userInfo.Picture,
            Sub = userInfo.Id,
            Username = userInfo.Name
        };
        
        var (userResponse, refreshToken) = await sender.Send(user.Adapt<LoginUserRequest>());

        user.Id = userResponse.Id;
        
        context.Response.Cookies.Append("refresh-token", refreshToken, new()
        {
            Expires = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpiryTimeInDays),
            Domain = context.Request.Host.Host,
            Path = "/"
        });
        
        var token = jwtService.GenerateToken(user);
        
        return Results.Ok(new
        {
            access_token = token,
            user_info = userResponse
        });
    }

    public async Task<IResult> Refresh(HttpContext context, ISender sender, IOptions<JwtOptions> jwtOptions)
    {
        var accessToken = context.Request.Headers[HeaderNames.Authorization]
            .ToString()
            .Replace("Bearer ", string.Empty);

        if (accessToken is null)
        {
            return CustomResults.ErrorJson(400, [Error.Failure(description: "access token not found")]);
        }
        
        var refreshToken = context.Request.Cookies["refresh-token"];

        if (refreshToken is null)
        {
            return CustomResults.ErrorJson(401, [Error.Failure(description: "refresh token not found, reauthorize")]);
        }

        var response = await sender.Send(new RefreshTokenRequest
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });

        if (!response.IsError)
        {
            context.Response.Cookies.Delete("refresh-token");
            context.Response.Cookies.Append("refresh-token", response.Value.RefreshToken, new()
            {
                Expires = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpiryTimeInDays),
                Domain = context.Request.Host.Host,
                Path = "/"
            });
            return Results.Json(response.Value.AccessToken);
        }
        
        var error = response.FirstError;
            
        return CustomResults.ErrorJson(error.Type switch
        {
            ErrorType.Failure => 400,
            ErrorType.Unauthorized => 401,
            ErrorType.NotFound => 404,
            _ => 500
        }, [error]);
    }
    
    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("auth");

        group.MapGet("google", GoogleLoginHandler);
        group.MapGet("google/callback", GoogleCallbackHandler);
        group.MapGet("refresh", Refresh);

        return group;
    }
}