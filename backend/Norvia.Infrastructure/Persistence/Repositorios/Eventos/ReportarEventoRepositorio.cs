using Norvia.Application.Features.Eventos.ReportarEvento;
using Norvia.Domain.Entities.Accidentes;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Eventos;

public sealed class ReportarEventoRepositorio(NorviaDbContext db) : IReportarEventoRepositorio
{
    public void Agregar(Accidente evento) => db.Accidente.Add(evento);
}
