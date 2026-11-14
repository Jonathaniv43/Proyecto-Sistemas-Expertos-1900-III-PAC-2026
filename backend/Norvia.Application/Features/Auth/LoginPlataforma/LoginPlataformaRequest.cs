namespace Norvia.Application.Features.Auth.LoginPlataforma;

public sealed record LoginPlataformaRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
