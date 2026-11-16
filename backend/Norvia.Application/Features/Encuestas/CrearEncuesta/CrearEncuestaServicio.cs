using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Encuestas.CrearEncuesta.DTOs;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.CrearEncuesta;

/// <summary>
/// HU-030: crea una encuesta en borrador con su primera versión y copia las preguntas activas
/// de las categorías del rubro de la empresa (RN-10, RN-11, RN-12).
/// </summary>
public sealed class CrearEncuestaServicio(
    ICrearEncuestaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<CrearEncuestaResponseDto>> EjecutarAsync(
        CrearEncuestaRequestDto request, CancellationToken ct)
    {
        if (sesion.EmpresaId is not Guid empresaId || sesion.VinculoEmpresaId is not Guid vinculoId)
            return Errores.EmpresaSinRubro;

        var rubroId = await repositorio.BuscarRubroDeEmpresaAsync(empresaId, ct);
        if (rubroId is not short rubro)
            return Errores.EmpresaSinRubro;

        var categorias = await repositorio.ListarCategoriasActivasConPreguntasAsync(
            rubro, request.CategoriaRiesgoIds, ct);
        if (categorias.Count != request.CategoriaRiesgoIds.Count)
            return Errores.CategoriaInvalida;

        var encuesta = new Encuesta
        {
            EmpresaId = empresaId,
            Titulo = request.Titulo.Trim(),
            Estado = EstadoEncuesta.Borrador,
            CreadaPorId = vinculoId
        };

        var version = new EncuestaVersion
        {
            EmpresaId = empresaId,
            EncuestaId = encuesta.Id,
            NumeroVersion = 1,
            Instrucciones = string.IsNullOrWhiteSpace(request.Instrucciones) ? null : request.Instrucciones.Trim(),
            Estado = EstadoVersionEncuesta.Borrador,
            CreadaPorId = vinculoId
        };

        short orden = 1;
        foreach (var categoria in categorias.OrderBy(c => c.Nombre))
        {
            version.Categorias.Add(new EncuestaCategoria
            {
                EmpresaId = empresaId,
                EncuestaVersionId = version.Id,
                CategoriaRiesgoId = categoria.Id
            });

            foreach (var origen in categoria.Preguntas.OrderBy(p => p.Orden))
            {
                var pregunta = new EncuestaPregunta
                {
                    EmpresaId = empresaId,
                    EncuestaVersionId = version.Id,
                    PreguntaExploratoriaId = origen.Id,
                    Texto = origen.Texto,
                    TipoRespuesta = origen.TipoRespuesta,
                    Obligatoria = origen.Obligatoria,
                    PeligroId = origen.PeligroId,
                    Orden = orden++
                };

                foreach (var opcion in origen.Opciones.OrderBy(o => o.Orden))
                {
                    pregunta.Opciones.Add(new EncuestaOpcion
                    {
                        EmpresaId = empresaId,
                        EncuestaPreguntaId = pregunta.Id,
                        Texto = opcion.Texto,
                        Orden = opcion.Orden,
                        IndicaExposicion = opcion.IndicaExposicion
                    });
                }

                version.Preguntas.Add(pregunta);
            }
        }

        encuesta.Versiones.Add(version);
        repositorio.Agregar(encuesta);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return new CrearEncuestaResponseDto(encuesta.Id, encuesta.Titulo, encuesta.Estado, version.Id, null);
    }
}
