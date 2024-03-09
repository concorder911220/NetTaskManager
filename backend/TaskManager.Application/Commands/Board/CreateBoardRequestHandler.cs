using ErrorOr;
using Mapster;
using MediatR;
using TaskManager.Infrastructure;

namespace TaskManager.Application.Commands.Board;

public class BoardResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record CreateBoardRequest : IRequest<ErrorOr<BoardResponse>>
{
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
}

public class CreateBoardRequestHandler(AppDbContext appDbContext) 
    : IRequestHandler<CreateBoardRequest, ErrorOr<BoardResponse>>
{
    private readonly AppDbContext _appDbContext = appDbContext;

    public async Task<ErrorOr<BoardResponse>> Handle(CreateBoardRequest request, CancellationToken cancellationToken)
    {
        var user = _appDbContext.Users.FirstOrDefault(u => u.Id == request.UserId);

        if (user is null)
        {
            return Error.Unauthorized(description: "user does not exist");
        }

        var board = request.Adapt<Domain.Entities.Board>();
        board.CreatedAt = DateTime.UtcNow;

        await _appDbContext.Boards.AddAsync(board, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        
        return board.Adapt<BoardResponse>();
    }
}
