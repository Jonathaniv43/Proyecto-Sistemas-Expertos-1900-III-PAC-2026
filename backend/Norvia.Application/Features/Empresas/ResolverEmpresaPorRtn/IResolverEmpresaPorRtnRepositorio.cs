namespace Norvia.Application.Features.Empresas.ResolverEmpresaPorRtn;

public sealed record EmpresaPorRtnLectura(Guid Id, string RazonSocial, string Rtn);

public interface IResolverEmpresaPorRtnRepositorio
{
    Task<EmpresaPorRtnLectura?> BuscarActivaPorRtnAsync(string rtn, CancellationToken cancellationToken);
}