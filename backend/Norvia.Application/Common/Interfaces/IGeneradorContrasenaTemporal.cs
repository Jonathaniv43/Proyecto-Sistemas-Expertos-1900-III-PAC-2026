namespace Norvia.Application.Common.Interfaces;

/// <summary>Genera contraseñas temporales para altas (empresa, vínculo, etc.).</summary>
public interface IGeneradorContrasenaTemporal
{
    string Generar();
}