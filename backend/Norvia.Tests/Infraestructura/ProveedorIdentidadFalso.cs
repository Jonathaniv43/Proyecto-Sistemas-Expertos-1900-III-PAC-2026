using System.Collections.Concurrent;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Domain.Enums;

namespace Norvia.Tests.Infraestructura;

/// <summary>
/// Reemplaza a Microsoft en las pruebas: cada "código de autorización" se registra con la identidad que
/// devolvería el proveedor. Nada sale del proceso.
/// </summary>
public sealed class ProveedorIdentidadFalso : IProveedorIdentidad
{
    private readonly ConcurrentDictionary<string, IdentidadExterna> _codigos = new();

    public ProveedorIdentidad Proveedor => ProveedorIdentidad.Microsoft;

    /// <summary>Registra una identidad y devuelve el código que la canjea.</summary>
    public string Registrar(string email, string userId, string nombre = "Mariana", string apellido = "Lopez")
    {
        var codigo = Guid.NewGuid().ToString("N");
        _codigos[codigo] = new IdentidadExterna(ProveedorIdentidad.Microsoft, userId, email.ToLowerInvariant(), nombre, null, apellido, null);
        return codigo;
    }

    public Task<Resultado<IdentidadExterna>> CanjearCodigoAsync(
        string codigo, string codeVerifier, string redirectUri, CancellationToken cancellationToken) =>
        Task.FromResult(_codigos.TryGetValue(codigo, out var identidad)
            ? Resultado<IdentidadExterna>.Exito(identidad)
            : Resultado<IdentidadExterna>.Fallo(new Error("auth.identidad_invalida", "Código inválido.", TipoError.NoAutorizado)));
}
