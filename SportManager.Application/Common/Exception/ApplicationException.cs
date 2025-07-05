namespace SportManager.Application.Common.Exception;

public class ApplicationException : System.Exception
{
    public ApplicationException() : base()
    {
    }

    public ApplicationException(string errorKey, string? message = null) : base(
        $"ERROR KEY: {errorKey}, message: {message}")
    {
        ErrorKey = errorKey;
    }

    public string ErrorKey { get; set; }
}
