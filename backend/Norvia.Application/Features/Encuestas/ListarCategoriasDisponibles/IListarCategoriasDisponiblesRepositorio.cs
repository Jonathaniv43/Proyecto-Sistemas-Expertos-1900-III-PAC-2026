using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Encuestas.ListarCategoriasDisponibles;

public interface IListarCategoriasDisponiblesRepositorio
{
    /// <summary>Rubro de la empresa indicada; nulo si todavía no lo tiene (perfil incompleto, HU-026).</summary>
    Task<short?> BuscarRubroDeEmpresaAsync(Guid empresaId, CancellationToken ct);

    /// <summary>Categorías activas del rubro, con sus preguntas activas y las opciones de cada una.</summary>
    Task<List<CategoriaRiesgo>> ListarActivasDelRubroAsync(short rubroId, CancellationToken ct);
}
