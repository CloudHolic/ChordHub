using ChordHub.Api.Shared.Models;

using Microsoft.AspNetCore.Mvc;

namespace ChordHub.Api.Shared.Extensions;

public static class ControllerExtensions
{
    public static IActionResult Success<T>(this ControllerBase controller, T data, string? message = null) =>
        controller.Ok(new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        });

    public static IActionResult Success(this ControllerBase controller, string? message = null) =>
        controller.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = message
        });

    public static IActionResult Error(this ControllerBase controller, string message, int statusCode = 400)
    {
        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Errors = [message]
        };

        return statusCode switch
        {
            400 => controller.BadRequest(response),
            401 => controller.Unauthorized(response),
            404 => controller.NotFound(response),
            _ => controller.StatusCode(statusCode, response)
        };
    }

    public static IActionResult PagedSuccess<T>(this ControllerBase controller, List<T> data, int page, int pageSize, int totalCount, string? message = null) =>
        controller.Ok(new PagedResponse<List<T>>
        {
            Success = true,
            Data = data,
            Message = message,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
}
