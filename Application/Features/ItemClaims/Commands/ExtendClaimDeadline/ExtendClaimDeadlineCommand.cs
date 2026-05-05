using Application.Core;
using MediatR;

namespace Application.Features.ItemClaims.Commands.ExtendClaimDeadline;

public class ExtendClaimDeadlineCommand : IRequest<Result<Unit>>
{
    public required string Id { get; set; }
}
