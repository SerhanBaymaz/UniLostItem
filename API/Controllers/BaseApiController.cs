using API.Responses;
using Application.Core;
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
                var notFoundResponse = StandardApiResponse<T>.NotFoundResponse(
                    result.Message ?? "Resource not found",
                    HttpContext.Request?.Path.ToString(),
                    traceId
                );
                return NotFound(notFoundResponse);
            }

            if (result.IsSuccess)
            {
                var successResponse = StandardApiResponse<T>.SuccessResponse(
                    result.Value!,
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

        /// <summary>
        /// Creates a standardized success response
        /// </summary>
        protected ActionResult<StandardApiResponse<T>> Success<T>(
            T data,
            string? message = null,
            int statusCode = 200)
        {
            var response = StandardApiResponse<T>.SuccessResponse(
                data,
                message,
                statusCode
            );
            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Creates a standardized success response without data
        /// </summary>
        protected ActionResult<StandardApiResponse<object>> Success(
            string message = "Operation completed successfully",
            int statusCode = 200)
        {
            var response = StandardApiResponse<object>.SuccessResponse(
                message,
                statusCode
            );
            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Creates a standardized error response
        /// </summary>
        protected ActionResult<StandardApiResponse<T>> Error<T>(
            string message,
            int statusCode = 400,
            string? type = null,
            string? detail = null)
        {
            var response = StandardApiResponse<T>.ErrorResponse(
                message,
                statusCode,
                type,
                detail,
                HttpContext.Request?.Path.ToString(),
                traceId: GetTraceId()
            );
            return StatusCode(statusCode, response);
        }
    }
}
