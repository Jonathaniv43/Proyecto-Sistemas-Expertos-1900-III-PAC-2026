namespace Norvia.Application.Common.Resultados;

/// <summary>Fallo de negocio esperado. No es una excepción.</summary>
public sealed record Error(string Codigo, string Mensaje, TipoError Tipo);
