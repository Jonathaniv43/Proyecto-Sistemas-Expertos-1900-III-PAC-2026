using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Empresas.ActualizarPerfilEmpresa.DTOs;
using Norvia.Application.Features.Empresas.DTOs;

namespace Norvia.Application.Features.Empresas.ActualizarPerfilEmpresa;

/// <summary>
/// Actualiza de forma parcial el perfil de la empresa de la sesión (HU-026).
/// No modifica RTN ni Estado. El rubro se resuelve por código contra la tabla Rubro.
/// </summary>
public sealed class ActualizarPerfilEmpresaServicio(
    IActualizarPerfilEmpresaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<PerfilEmpresaResponseDto>> EjecutarAsync(
        ActualizarPerfilEmpresaRequestDto request, CancellationToken ct)
    {
        if (sesion.EmpresaId is not Guid empresaId) return Errores.EmpresaNoEncontrada;

        var empresa = await repositorio.BuscarAsync(empresaId, ct);
        if (empresa is null) return Errores.EmpresaNoEncontrada;

        if (request.RazonSocial is not null)
            empresa.RazonSocial = request.RazonSocial.Trim();

        if (request.Direccion is not null)
            empresa.Direccion = string.IsNullOrWhiteSpace(request.Direccion) ? null : request.Direccion.Trim();

        if (request.ActividadEconomica is not null)
            empresa.ActividadEconomica = string.IsNullOrWhiteSpace(request.ActividadEconomica)
                ? null
                : request.ActividadEconomica.Trim();

        if (request.RubroCodigo is not null)
        {
            var codigo = request.RubroCodigo.Trim().ToLowerInvariant();
            var rubroId = await repositorio.BuscarRubroIdActivoAsync(codigo, ct);
            if (rubroId is null) return Errores.RubroInvalido;
            empresa.RubroId = rubroId;
            empresa.Rubro = await repositorio.BuscarRubroAsync(rubroId.Value, ct);
        }

        if (request.TrabajadoresPermanentes is not null)
            empresa.TrabajadoresPermanentes = request.TrabajadoresPermanentes;

        if (request.FechaInicioOperaciones is not null)
            empresa.FechaInicioOperaciones = request.FechaInicioOperaciones;

        await unidadDeTrabajo.GuardarCambiosAsync(ct);
        return PerfilEmpresaResponseDto.De(empresa);
    }
}
