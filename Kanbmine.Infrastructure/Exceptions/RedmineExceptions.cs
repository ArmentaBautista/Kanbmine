using System.Net;

namespace Kanbmine.Infrastructure.Exceptions;

public class RedmineApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    
    public RedmineApiException(string message) : base(message) { }
    
    public RedmineApiException(string message, Exception inner) 
        : base(message, inner) { }
    
    public RedmineApiException(string message, HttpStatusCode statusCode) 
        : base(message)
    {
        StatusCode = statusCode;
    }
}

public class RedmineAuthException : RedmineApiException
{
    public RedmineAuthException(string message) : base(message) { }
}

public class RedmineValidationException : RedmineApiException
{
    public List<string> Errors { get; }
    
    public RedmineValidationException(List<string> errors) 
        : base("Error de validación")
    {
        Errors = errors;
    }
}
