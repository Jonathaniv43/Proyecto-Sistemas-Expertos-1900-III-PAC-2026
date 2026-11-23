using Norvia.Domain.Entities.Riesgos;

namespace Norvia.Application.Features.Encuestas.RevisarPeligroSugerido;

public interface IRevisarPeligroSugeridoRepositorio
{
    /// <summary>Riesgo registrado a partir de la encuesta indicada (con seguimiento de cambios), o null.</summary>
    Task<RiesgoEmpresa?> BuscarAsync(Guid encuestaId, Guid riesgoId, CancellationToken ct);
}
