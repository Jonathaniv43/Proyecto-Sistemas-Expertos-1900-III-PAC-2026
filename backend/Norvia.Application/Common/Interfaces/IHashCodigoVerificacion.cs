namespace Norvia.Application.Common.Interfaces;

/// <summary>
/// Genera y comprueba el código de 6 dígitos de <see cref="Norvia.Domain.Entities.Acceso.VerificacionCorreoMicrosoft"/>
/// (D29, RN-86). El código en claro nunca se guarda: solo viaja en el correo y se compara contra su hash.
/// </summary>
public interface IHashCodigoVerificacion
{
    /// <summary>Un código aleatorio de 6 dígitos (con ceros a la izquierda).</summary>
    string Generar();

    string Hashear(string codigo);

    /// <summary>Compara en tiempo constante, para no filtrar por temporizado cuántos dígitos coinciden.</summary>
    bool Coincide(string hash, string codigo);
}
