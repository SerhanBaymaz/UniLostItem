using API.Controllers.Requests;
using API.Responses;
using Application.Core.Pagination;
using Application.Features.ItemClaims.Commands.AdminReviewClaim;
using Application.Features.ItemClaims.Commands.CancelItemClaim;
using Application.Features.ItemClaims.Commands.CreateItemClaim;
using Application.Features.ItemClaims.Commands.ExtendClaimDeadline;
using Application.Features.ItemClaims.Commands.RespondToClaim;
using Application.Features.ItemClaims.Queries.Common.DTOs;
using Application.Features.ItemClaims.Queries.GetClaimDetails;
using Application.Features.ItemClaims.Queries.GetClaimsByItem;
using Application.Features.ItemClaims.Queries.GetMyClaims;
using Application.Features.ItemClaims.Queries.GetPendingClaims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/claims")]
[ApiController]
public class ItemClaimsController : BaseApiController
{
    [HttpGet("by-item/{lostItemId}")]
    [Authorize]
    [ProducesResponseType(typeof(StandardApiResponse<PaginatedListDto<GetItemClaimDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<StandardApiResponse<PaginatedListDto<GetItemClaimDto>>>> GetClaimsByItem(
        string lostItemId,
        [FromQuery] GetClaimsByItemRequest request)
    {
        var query = new GetClaimsByItemQuery
        {
            LostItemId = lostItemId,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Status = request.Status
        };
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("my-claims")]
    [Authorize]
    [ProducesResponseType(typeof(StandardApiResponse<PaginatedListDto<GetItemClaimDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<StandardApiResponse<PaginatedListDto<GetItemClaimDto>>>> GetMyClaims(
        [FromQuery] GetMyClaimsRequest request)
    {
        var query = new GetMyClaimsQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Status = request.Status
        };
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<StandardApiResponse<GetItemClaimDto>>> GetClaimDetails(string id)
    {
        return HandleResult(await Mediator.Send(new GetClaimDetailsQuery { Id = id }));
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(StandardApiResponse<PaginatedListDto<GetItemClaimDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<StandardApiResponse<PaginatedListDto<GetItemClaimDto>>>> GetPendingClaims(
        [FromQuery] GetPendingClaimsRequest request)
    {
        var query = new GetPendingClaimsQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            SearchTerm = request.SearchTerm
        };
        return HandleResult(await Mediator.Send(query));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<StandardApiResponse<string>>> CreateClaim(
        [FromBody] CreateItemClaimDto createItemClaimDto)
    {
        var command = new CreateItemClaimCommand { CreateItemClaimDto = createItemClaimDto };
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}/cancel")]
    [Authorize]
    public async Task<ActionResult<StandardApiResponse<Unit>>> CancelClaim(string id)
    {
        return HandleResult(await Mediator.Send(new CancelItemClaimCommand { Id = id }));
    }

    [HttpPut("{id}/respond")]
    [Authorize]
    public async Task<ActionResult<StandardApiResponse<Unit>>> RespondToClaim(
        string id, [FromBody] RespondToClaimDto respondToClaimDto)
    {
        var command = new RespondToClaimCommand { Id = id, RespondToClaimDto = respondToClaimDto };
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}/extend")]
    [Authorize]
    public async Task<ActionResult<StandardApiResponse<Unit>>> ExtendClaimDeadline(string id)
    {
        return HandleResult(await Mediator.Send(new ExtendClaimDeadlineCommand { Id = id }));
    }

    [HttpPut("{id}/admin-review")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<StandardApiResponse<Unit>>> AdminReviewClaim(
        string id, [FromBody] AdminReviewClaimDto adminReviewClaimDto)
    {
        var command = new AdminReviewClaimCommand { Id = id, AdminReviewClaimDto = adminReviewClaimDto };
        return HandleResult(await Mediator.Send(command));
    }
}
