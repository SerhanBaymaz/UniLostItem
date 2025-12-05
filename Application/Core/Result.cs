using System;

namespace Application.Core;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? Message { get; set; }
    public int Code { get; set; }

    public static Result<T> Success(string message, T value) => new()
    {
        IsSuccess = true,
        Value = value,
        Message = message
    };

    public static Result<T> Failure(string message, int code) => new()
    {
        IsSuccess = false,
        Message = message,
        Code = code
    };
}
