using Norvia.Domain.Entities.Accidentes;

namespace Norvia.Application.Features.Eventos.ReportarEvento;

public interface IReportarEventoRepositorio
{
    void Agregar(Accidente evento);
}
