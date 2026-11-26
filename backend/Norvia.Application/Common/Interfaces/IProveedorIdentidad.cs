using Norvia.Application.Common.Resultados;
using Norvia.Domain.Enums;

namespace Norvia.Application.Common.Interfaces;

/// <summary>Identidad que entrega un proveedor externo tras canjear el código de autorización.</summary>
public sealed record IdentidadExterna(
    ProveedorIdentidad Proveedor,
    string ProveedorUserId,
    string Email,
    string PrimerNombre,
    string? SegundoNombre,
    string PrimerApellido,
    string? SegundoApellido,
    /// <summary>true si el dueño del correo lo confirmó con el código de 6 dígitos (RN-86). Microsoft no lo da por probado.</summary>
    bool CorreoVerificado = false);

/// <summary>
/// Proveedor de identidad externo (D29). Agregar Google es agregar otra implementación y un valor en
/// <see cref="ProveedorIdentidad"/>, sin tocar los casos de uso.
/// </summary>
public interface IProveedorIdentidad
{
    ProveedorIdentidad Proveedor { get; }

    /// <summary>Canjea el código (flujo OAuth con PKCE) y valida la identidad. Nunca confía en datos del cliente.</summary>
    Task<Resultado<IdentidadExterna>> CanjearCodigoAsync(
        string codigo, string codeVerifier, string redirectUri, CancellationToken cancellationToken);
}
