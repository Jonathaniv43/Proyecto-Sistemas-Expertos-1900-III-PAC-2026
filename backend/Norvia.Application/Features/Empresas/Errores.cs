using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Empresas;

/// <summary>Errores de negocio del módulo Empresas, compartidos solo por sus slices.</summary>
public static class Errores
{
    public static readonly Error RtnDuplicado = new("empresas.rtn_duplicado", "Ya existe una empresa registrada con ese RTN.", TipoError.Conflicto);

    public static readonly Error EmailDuplicado = new("empresas.email_duplicado", "Ya existe una cuenta registrada con ese correo electrónico.", TipoError.Conflicto);
}