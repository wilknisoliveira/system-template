namespace UserInterface.ExceptionHandler;

public class ExceptionResponse(int statusCode, string typeName, string message)
{
    public int StatusCode { get; set; } = statusCode;
    public string TypeName { get; set; } = typeName;
    public string Message { get; set; } = message;
}