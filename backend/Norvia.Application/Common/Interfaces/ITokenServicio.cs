using Norvia.Application.Common.Seguridad;

namespace Norvia.Application.Common.Interfaces;

/// <summary>Emisión del JWT de acceso y del refresh token (en BD solo se guarda su hash).</summary>
public interface ITokenServicio
{
    string CrearAccess(SesionToken sesion, DateTime ahoraUtc);
    (string TokenPlano, string Hash) CrearRefresh();
    string HashRefresh(string tokenPlano);
    int AccessSegundos { get; }
    TimeSpan RefreshDuracion { get; }
}
