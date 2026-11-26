namespace Norvia.Application.Features.Empresas.ResolverEmpresaPorRtn.DTOs;

/// <summary>Consulta anónima previa al login empresa (landing).</summary>
public sealed record ResolverEmpresaPorRtnRequestDto
{
    public string Rtn { get; init; } = string.Empty;
}