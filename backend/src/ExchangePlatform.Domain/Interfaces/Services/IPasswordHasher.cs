namespace ExchangePlatform.Domain.Interfaces.Services;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verificar(string password, string hash);
}