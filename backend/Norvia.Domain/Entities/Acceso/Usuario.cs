namespace Norvia.Domain.Entities.Acceso;

/// <summary>Indica si la persona puede usar la plataforma. No es el login (D21).</summary>
public class Usuario
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid PersonaId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; }

    public Persona Persona { get; set; } = null!;
}
