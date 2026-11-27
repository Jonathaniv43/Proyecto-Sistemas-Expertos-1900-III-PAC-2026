using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Norvia.Application.Common.Interfaces;
using Norvia.Domain.Entities.Auditoria;

namespace Norvia.Application.Common.Auditoria;

/// <summary>
/// Registra evidencias en <see cref="RegistroAuditoria"/> dentro de la misma unidad de trabajo del caso de uso (HU-099).
/// No confirma cambios por su cuenta: si la operación de origen falla, la auditoría tampoco se guarda.
/// </summary>
public sealed class Auditor(IRegistroAuditoria repositorio, ITenantActual sesion)
{
    private static readonly JsonSerializerOptions OpcionesJson = ArmarOpcionesJson();

    public void Registrar(
        string accion,
        string entidad,
        Guid entidadId,
        object? detalle = null,
        Guid? vinculoEmpresaId = null,
        Guid? empresaId = null)
    {
        var entrada = new RegistroAuditoria
        {
            EmpresaId = empresaId ?? sesion.EmpresaId ?? Guid.Empty,
            VinculoEmpresaId = vinculoEmpresaId ?? sesion.VinculoEmpresaId,
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            Detalle = detalle is null ? null : JsonSerializer.Serialize(detalle, OpcionesJson)
        };

        repositorio.Agregar(entrada);
    }

    private static JsonSerializerOptions ArmarOpcionesJson()
    {
        var opciones = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        opciones.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false));
        return opciones;
    }
}
