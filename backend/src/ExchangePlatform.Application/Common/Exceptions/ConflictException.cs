namespace ExchangePlatform.Application.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string mensaje) : base(mensaje) { }
}