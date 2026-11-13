namespace Norvia.Application.Features.Auth.DTOs;

public class ResultadoAuth
{
    public bool Ok { get; init; }
    public TokenResponse? Tokens { get; init; }

    public static ResultadoAuth Fallo() => new() { Ok = false };
    public static ResultadoAuth Exito(TokenResponse tokens) => new() { Ok = true, Tokens = tokens };
}
