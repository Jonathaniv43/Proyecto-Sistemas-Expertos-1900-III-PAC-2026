using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Vinculos.ActualizarVinculo.DTOs;

/// <summary>Edición parcial: se envía solo lo que cambia.</summary>
public sealed record ActualizarVinculoRequestDto
{
    /// <summary>Nuevo rol en esta empresa (RN-03).</summary>
    public string? RolCodigo { get; init; }

    /// <summary><c>activo</c>, <c>suspendido</c> o <c>baja</c>. Cada cambio queda en el historial (RN-59).</summary>
    public EstadoVinculo? Estado { get; init; }

    /// <summary>
    /// <c>decision_admin</c>, <c>baja_laboral</c>, <c>falta_pago_empresa</c> u <c>otro</c>.
    /// Obligatorio al suspender o dar de baja; al reactivar, si falta se registra <c>decision_admin</c>.
    /// </summary>
    public MotivoEstadoVinculo? MotivoCodigo { get; init; }
    public string? MotivoDetalle { get; init; }
}
