namespace ExchangePlatform.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(List<string> errors)
        : base("Se produjeron errores de validación.")
    {
        Errors = errors;
    }
}