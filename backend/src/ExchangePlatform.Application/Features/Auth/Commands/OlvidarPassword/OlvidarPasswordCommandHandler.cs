using System.Security.Cryptography;
using ExchangePlatform.Domain.Entities;
using ExchangePlatform.Domain.Interfaces;
using ExchangePlatform.Domain.Interfaces.Services;
using MediatR;

namespace ExchangePlatform.Application.Features.Auth.Commands.OlvidarPassword;

public class OlvidarPasswordCommandHandler
    : IRequestHandler<OlvidarPasswordCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _email;
    private readonly IUrlSettings _urlSettings;

    public OlvidarPasswordCommandHandler(
        IUnitOfWork uow, IEmailService email, IUrlSettings urlSettings)
    {
        _uow = uow;
        _email = email;
        _urlSettings = urlSettings;
    }

    public async Task<Unit> Handle(
        OlvidarPasswordCommand request, CancellationToken ct)
    {
        var usuario = await _uow.Usuarios.GetByEmailAsync(request.Email);

        // RN-061: la respuesta es identica exista o no el correo, para no
        // revelar que usuarios estan registrados. Si no existe, no hacemos nada mas.
        if (usuario == null)
            return Unit.Value;

        await _uow.PasswordResetTokens.InvalidarTodosDelUsuarioAsync(usuario.Id);

        var tokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var resetToken = new PasswordResetToken(
            tokenValue, usuario.Id, DateTime.UtcNow.AddHours(1));
        resetToken.CreatedBy = usuario.Id;

        await _uow.PasswordResetTokens.AddAsync(resetToken);
        await _uow.SaveChangesAsync(ct);

        var enlace = $"{_urlSettings.FrontendUrl}/reset-password?token={Uri.EscapeDataString(tokenValue)}";

        await _email.EnviarAsync(
            usuario.Email,
            "Recupera tu contraseña",
            $"Hola {usuario.Nombres}, para restablecer tu contraseña visita el siguiente enlace " +
            $"(valido por 1 hora): {enlace}");

        return Unit.Value;
    }
}
