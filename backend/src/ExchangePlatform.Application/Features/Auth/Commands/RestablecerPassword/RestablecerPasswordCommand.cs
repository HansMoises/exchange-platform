using MediatR;

namespace ExchangePlatform.Application.Features.Auth.Commands.RestablecerPassword;

public record RestablecerPasswordCommand(
    string Token,
    string Password,
    string ConfirmPassword) : IRequest<Unit>;
