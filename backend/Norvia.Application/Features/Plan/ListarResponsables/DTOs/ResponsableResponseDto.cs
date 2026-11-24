namespace Norvia.Application.Features.Plan.ListarResponsables.DTOs;

/// <summary>Persona que puede ser responsable de una tarea: vínculo activo de la empresa (HU-043, RN-07).</summary>
public sealed record ResponsableResponseDto(Guid Id, string NombreCompleto, string RolCodigo);
