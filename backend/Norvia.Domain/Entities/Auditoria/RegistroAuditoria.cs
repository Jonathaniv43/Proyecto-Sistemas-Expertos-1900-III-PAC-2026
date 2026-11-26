using Norvia.Domain.Common;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Empresas;

namespace Norvia.Domain.Entities.Auditoria;

/// <summary>
/// Evidencia cronológica e inalterable de acciones importantes en la empresa (HU-099, 12-modelo-datos § RegistroAuditoria).
/// La aplicación solo inserta; nunca actualiza ni borra.
/// </summary>
public class RegistroAuditoria : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid? VinculoEmpresaId { get; set; }
    public Guid? StaffPlataformaId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public Guid EntidadId { get; set; }
    public DateTime Fecha { get; set; }
    public string? Detalle { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public VinculoEmpresa? VinculoEmpresa { get; set; }
    public StaffPlataforma? StaffPlataforma { get; set; }
}
