namespace Norvia.Application.Features.AccesoRecurso.PermitirAccesoRecurso;

/// <summary>Datos HTTP ya extraídos por la API; Application no conoce HttpContext.</summary>
public sealed record ContextoAccesoRecurso(
    bool OmitirVerificacion,
    bool EstaAutenticado,
    string MetodoHttp,
    string RutaPatron,
    Guid? AccesoId);
