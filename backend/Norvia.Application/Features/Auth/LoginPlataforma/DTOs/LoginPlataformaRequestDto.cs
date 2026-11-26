namespace Norvia.Application.Features.Auth.LoginPlataforma.DTOs;

public sealed record LoginPlataformaRequestDto
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
