using ExchangePlatform.Application.Features.Auth.DTOs;
using MediatR;

namespace ExchangePlatform.Application.Features.Auth.Commands.IniciarSesion;

public record IniciarSesionCommand(
    string Email,
    string Password) : IRequest<LoginResponseDto>;