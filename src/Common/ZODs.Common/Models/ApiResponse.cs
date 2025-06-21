using System.Net;

namespace ZODs.Common.Models;

public class ApiResponse
{
    public int StatusCode { get; set; }

    public string? Message { get; set; }

    public object? Data { get; set; }

    public static ApiResponse Create(object data, string message, int statusCode)
    {
        return new ApiResponse
        {
            Data = data,
            Message = message,
            StatusCode = statusCode,
        };
    }

    public static ApiResponse Create(object data)
    {
        return new ApiResponse
        {
            Data = data,
            Message = string.Empty,
            StatusCode = 200,
        };
    }

    public static ApiResponse Error(string message, HttpStatusCode statusCode)
    {
        return new ApiResponse
        {
            Data = null,
            Message = message,
            StatusCode = (int)statusCode,
        };
    }
}
