using API.Responses;
using Application.Core;
using Application.Core.Pagination;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        private IMediator? _mediator;

        protected IMediator Mediator =>
            _mediator ??= HttpContext.RequestServices.GetService<IMediator>()
                ?? throw new InvalidOperationException("IMediator service is unavailable");

        /// <summary>
        /// Gets the current trace identifier (W3C Trace Context)
        /// </summary>
        protected string GetTraceId() => Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier;

        /// <summary>
        /// Handles Result<T> and wraps it in StandardApiResponse
        /// </summary>
        protected ActionResult<StandardApiResponse<T>> HandleResult<T>(Result<T> result)
        {
            var traceId = GetTraceId();

            if (!result.IsSuccess && result.Code == 404)
            {
                var notFoundResponse = StandardApiResponse<T>.ErrorResponse(
                    result.Message ?? "Resource not found",
                    404,
                    "NotFound",
                    result.Message ?? "The requested resource was not found",
                    HttpContext.Request?.Path.ToString(),
                    traceId: traceId
                );
                return NotFound(notFoundResponse);
            }

            if (result.IsSuccess && result.Value is not null)
            {
                // Check if result contains PaginatedListDto
                if (result.Value is IPaginatedList paginated)
                {
                    var metadata = new Dictionary<string, object?>
                    {
                        ["totalCount"] = paginated.TotalCount,
                        ["pageNumber"] = paginated.PageNumber,
                        ["pageSize"] = paginated.PageSize,
                        ["totalPages"] = paginated.TotalPages,
                        ["hasPrevious"] = paginated.HasPrevious,
                        ["hasNext"] = paginated.HasNext
                    };

                    // Get the Items from the paginated list
                    var items = paginated.GetItems();
                    var response = StandardApiResponse<object>.SuccessResponse(
                        items!,
                        result.Message,
                        statusCode: 200,
                        metadata: metadata);

                    return Ok(response);
                }

                // Non-paginated result (existing behavior)
                var successResponse = StandardApiResponse<T>.SuccessResponse(
                    result.Value,
                    result.Message ?? "Request completed successfully"
                );
                return Ok(successResponse);
            }

            var errorResponse = StandardApiResponse<T>.ErrorResponse(
                result.Message ?? "Bad request",
                400,
                "BadRequest",
                result.Message,
                HttpContext.Request?.Path.ToString(),
                traceId: traceId
            );
            return BadRequest(errorResponse);
        }
    }
}
