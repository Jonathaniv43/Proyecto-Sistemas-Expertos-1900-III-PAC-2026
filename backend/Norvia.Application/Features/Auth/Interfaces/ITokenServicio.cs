using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Auth.Interfaces;

public record SesionToken(
    Guid AccesoId,
    TipoSesion Tipo,
    Guid? VinculoEmpresaId,
    Guid? EmpresaId,
    string? RolCodigo);

public interface ITokenServicio
{
    string CrearAccess(SesionToken sesion, DateTime ahoraUtc);
    (string TokenPlano, string Hash) CrearRefresh();
    string HashRefresh(string tokenPlano);
    int AccessSegundos { get; }
    TimeSpan RefreshDuracion { get; }
}
