namespace ExchangePlatform.Application.Common.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string mensaje) : base(mensaje) { }
}