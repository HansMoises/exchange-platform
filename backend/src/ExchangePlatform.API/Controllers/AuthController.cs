using ExchangePlatform.Application.Common.Models;
using ExchangePlatform.Application.Features.Auth.Commands.CerrarSesion;
using ExchangePlatform.Application.Features.Auth.Commands.IniciarSesion;
using ExchangePlatform.Application.Features.Auth.Commands.OlvidarPassword;
using ExchangePlatform.Application.Features.Auth.Commands.RegistrarUsuario;
using ExchangePlatform.Application.Features.Auth.Commands.RenovarToken;
using ExchangePlatform.Application.Features.Auth.Commands.RestablecerPassword;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExchangePlatform.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarUsuarioCommand command)
    {
        var id = await _mediator.Send(command);
        return Created($"/api/v1/users/{id}",
            ApiResponse<object>.Ok(new { id }, "Cuenta creada exitosamente."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> IniciarSesion(
        [FromBody] IniciarSesionCommand command)
    {
        var resultado = await _mediator.Send(command);
        return Ok(ApiResponse<object>.Ok(resultado, "Inicio de sesión exitoso."));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RenovarToken(
        [FromBody] RenovarTokenCommand command)
    {
        var resultado = await _mediator.Send(command);
        return Ok(ApiResponse<object>.Ok(resultado, "Token renovado exitosamente."));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> CerrarSesion(
        [FromBody] CerrarSesionCommand command)
    {
        await _mediator.Send(command);
        return Ok(ApiResponse<object>.Ok(null, "Sesión cerrada exitosamente."));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> OlvidarPassword(
        [FromBody] OlvidarPasswordCommand command)
    {
        await _mediator.Send(command);
        return Ok(ApiResponse<object>.Ok(
            null, "Si el correo está registrado, recibirás instrucciones en breve."));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> RestablecerPassword(
        [FromBody] RestablecerPasswordCommand command)
    {
        await _mediator.Send(command);
        return Ok(ApiResponse<object>.Ok(null, "Contraseña actualizada exitosamente."));
    }
}