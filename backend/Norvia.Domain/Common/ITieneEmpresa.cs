namespace Norvia.Domain.Common;

/// <summary>
/// Marca las entidades que pertenecen a una empresa (tenant).
/// Infrastructure las usa para aplicar HasQueryFilter y asignar EmpresaId al guardar.
/// </summary>
public interface ITieneEmpresa
{
    Guid EmpresaId { get; set; }
}
