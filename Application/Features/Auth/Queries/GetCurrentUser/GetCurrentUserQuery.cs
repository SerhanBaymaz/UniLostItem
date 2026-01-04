using Application.Core;
using MediatR;

namespace Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQuery : IRequest<Result<CurrentUserDto>>
{
    // No input needed - user is determined from JWT token in the request context.
}
