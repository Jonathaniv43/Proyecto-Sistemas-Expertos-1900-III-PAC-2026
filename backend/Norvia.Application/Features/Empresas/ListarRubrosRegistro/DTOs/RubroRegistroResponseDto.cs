namespace Norvia.Application.Features.Empresas.ListarRubrosRegistro.DTOs;

/// <summary>Opción del selector de rubro en el registro: solo lo que necesita la persona para elegir.</summary>
public sealed record RubroRegistroResponseDto(string Codigo, string Nombre);
