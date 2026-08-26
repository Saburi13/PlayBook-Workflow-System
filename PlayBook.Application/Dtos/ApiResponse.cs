namespace PlayBook.Application.Dtos;

public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public T? Data { get; set; }
    public string Message { get; set; } = "Operation completed successfully";
    public Dictionary<string, string[]> Errors { get; set; } = new();
}

public class ApiResponse : ApiResponse<object>
{
}
