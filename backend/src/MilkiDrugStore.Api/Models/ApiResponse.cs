namespace MilkiDrugStore.Api.Models;

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public List<string>? Errors { get; set; }
    public string? CorrelationId { get; set; }

    public static ApiResponse Ok(object? data = null, string? message = null)
    {
        return new ApiResponse { Success = true, Data = data, Message = message ?? "Operation successful" };
    }

    public static ApiResponse Fail(string message, int statusCode = StatusCodes.Status400BadRequest)
    {
        return new ApiResponse { Success = false, Message = message, Errors = new List<string> { message } };
    }
}
