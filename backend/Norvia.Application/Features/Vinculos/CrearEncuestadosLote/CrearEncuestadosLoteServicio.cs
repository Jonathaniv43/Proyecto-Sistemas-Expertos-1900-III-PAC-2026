using FluentValidation;
using Norvia.Application.Common.Auditoria;
using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Features.Vinculos.CrearEncuestadosLote.DTOs;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Vinculos.CrearEncuestadosLote;

/// <summary>
/// Alta de encuestados por lote (HU-115). Cada fila válida crea o reutiliza la cuenta y recibe el rol
/// <c>encuestado</c>; las filas con problemas se informan y no detienen a las demás. Las cuentas nuevas
/// reciben un código de 6 dígitos por correo para activarlas. Todo se guarda en una transacción.
/// </summary>
public sealed class CrearEncuestadosLoteServicio(
    AltaDeVinculo alta,
    IValidator<FilaEncuestadoDto> validadorFila,
    IUnidadDeTrabajo unidadDeTrabajo,
    Auditor auditor) : IServicioCasoDeUso
{
    public async Task<CrearEncuestadosLoteResponseDto> EjecutarAsync(CrearEncuestadosLoteRequestDto request, CancellationToken ct)
    {
        var preparados = new List<(int Fila, VinculoPreparado Datos)>();
        var errores = new List<ErrorFilaDto>();
        var correos = new HashSet<string>();

        for (var i = 0; i < request.Filas.Count; i++)
        {
            var numero = i + 1;
            var fila = request.Filas[i];
            if (fila is null)
            {
                errores.Add(new(numero, "", "vinculos.fila_invalida", "La fila está vacía."));
                continue;
            }

            var validacion = await validadorFila.ValidateAsync(fila, ct);
            if (!validacion.IsValid)
            {
                var codigo = validacion.Errors.Any(e => e.PropertyName == nameof(FilaEncuestadoDto.Email))
                    ? "vinculos.email_invalido"
                    : "vinculos.datos_invalidos";
                errores.Add(new(numero, fila.Email, codigo, string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage))));
                continue;
            }

            if (!correos.Add(fila.Email.Trim().ToLowerInvariant()))
            {
                errores.Add(new(numero, fila.Email, "vinculos.duplicado_en_lote", "El correo ya aparece en otra fila del lote."));
                continue;
            }

            var datos = new DatosPersonaAlta(fila.Email, fila.PrimerNombre, fila.SegundoNombre, fila.PrimerApellido, fila.SegundoApellido);
            var preparado = await alta.PrepararAsync(datos, Rol.EncuestadoId, "Encuestado", ct);
            if (preparado.EsExito)
                preparados.Add((numero, preparado.Valor));
            else
                errores.Add(new(numero, fila.Email, preparado.Error!.Codigo, preparado.Error.Mensaje));
        }

        if (preparados.Count > 0)
        {
            foreach (var (_, datos) in preparados)
            {
                auditor.Registrar(AccionesAuditoria.EncuestadoCreado, EntidadesAuditoria.VinculoEmpresa, datos.Vinculo.Id,
                    new { persona = datos.Persona.NombreCompleto, email = datos.Acceso.Email, rol = "encuestado", datos.CuentaNueva, lote = true });
            }
            await unidadDeTrabajo.GuardarCambiosAsync(ct);
        }

        // Los correos salen solo después de confirmar el lote: si el guardado falla, no queda ningún código suelto.
        var cuentas = new List<EncuestadoCreadoDto>(preparados.Count);
        foreach (var (fila, datos) in preparados)
        {
            var enviado = await alta.EnviarActivacionAsync(datos, ct);
            cuentas.Add(new EncuestadoCreadoDto(fila, datos.Vinculo.Id, datos.Acceso.Email,
                datos.Persona.NombreCompleto, datos.CuentaNueva, enviado));
        }
        return new CrearEncuestadosLoteResponseDto(cuentas.Count, cuentas, errores);
    }
}
