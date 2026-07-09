using ExchangePlatform.Application.Features.Auth.DTOs;
using MediatR;

namespace ExchangePlatform.Application.Features.Auth.Commands.RenovarToken;

public record RenovarTokenCommand(string RefreshToken) : IRequest<LoginResponseDto>;