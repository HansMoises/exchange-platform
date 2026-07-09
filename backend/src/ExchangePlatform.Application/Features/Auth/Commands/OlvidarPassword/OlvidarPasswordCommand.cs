using MediatR;

namespace ExchangePlatform.Application.Features.Auth.Commands.OlvidarPassword;

public record OlvidarPasswordCommand(string Email) : IRequest<Unit>;
