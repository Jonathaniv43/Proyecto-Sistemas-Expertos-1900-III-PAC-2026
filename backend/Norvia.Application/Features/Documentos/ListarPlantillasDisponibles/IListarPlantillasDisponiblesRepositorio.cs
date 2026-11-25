namespace Norvia.Application.Features.Documentos.ListarPlantillasDisponibles;

public interface IListarPlantillasDisponiblesRepositorio
{
    /// <summary>El rubro de la empresa; nulo si el perfil todavía no lo tiene.</summary>
    Task<short?> RubroDeEmpresaAsync(Guid empresaId, CancellationToken ct);

    /// <summary>Plantillas activas del rubro o de todos los rubros, por tipo y título.</summary>
    Task<IReadOnlyList<PlantillaDisponibleResponseDto>> ListarAsync(short rubroId, CancellationToken ct);
}
