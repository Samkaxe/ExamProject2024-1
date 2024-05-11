namespace Auth.Domain.OperationResults;

public class OperationResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }

    // Factory methods for success/failure
    public static OperationResult<T> CreateSuccessResult(T data) => new OperationResult<T> { Success = true, Data = data };
    public static OperationResult<T> CreateFailure(string errorMessage) => new OperationResult<T> { Success = false, ErrorMessage = errorMessage };
}