using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Commands.Board;
using TaskManager.WebApi.Exceptions;
using TaskManager.WebApi.Extensions;
using TaskManager.WebApi.Services;

namespace TaskManager.WebApi.Modules;

public class BoardModule : IModule
{
    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("boards");
        group.MapPost("/", Create).RequireAuthorization();
        
        return group;
    }

    private async Task<IResult> Create(
        [FromBody] BoardRequest? boardRequest,
        [FromServices] IValidator<BoardRequest> validator,
        ISender sender,
        UserContext userContext)
    {
        await ApiException.ValidateAsync(validator, boardRequest);
        
        var id = userContext.UserId;

        var response = await sender.Send(boardRequest.Adapt<CreateBoardRequest>() with
        {
            UserId = id
        });

        if (!response.IsError) return Results.Json(response.Value);
        
        var error = response.FirstError;
        return CustomResults.ErrorJson(error.Type, [error]);
    }
}

public class BoardRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    
    public class Validator : AbstractValidator<BoardRequest>
    {
        public Validator()
        {
            RuleFor(b => b.Name).NotEmpty();
            RuleFor(b => b.Description).NotEmpty();
        }
    }
}