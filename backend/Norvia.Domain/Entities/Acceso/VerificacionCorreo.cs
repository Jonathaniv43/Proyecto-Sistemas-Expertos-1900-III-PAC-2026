namespace Norvia.Domain.Entities.Acceso;

/// <summary>
/// Código de 6 dígitos para confirmar un correo antes de dar acceso: al registrarse con un proveedor de identidad
/// (D29, RN-86, <see cref="Norvia.Domain.Enums.PropositoVerificacionCorreo.Microsoft"/>, sin <see cref="AccesoId"/>
/// porque todavía no hay Acceso) o al activar una cuenta local creada por alguien más
/// (<see cref="Norvia.Domain.Enums.PropositoVerificacionCorreo.ActivacionCuenta"/>, con <see cref="AccesoId"/>).
/// Global, sin EmpresaId. Reenviar el código actualiza la misma fila en vez de acumular filas.
/// </summary>
public class VerificacionCorreo
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Norvia.Domain.Enums.PropositoVerificacionCorreo Proposito { get; set; }
    public string Email { get; set; } = string.Empty;

    /// <summary>Solo <see cref="Norvia.Domain.Enums.PropositoVerificacionCorreo.Microsoft"/>.</summary>
    public string? ProveedorUserId { get; set; }

    /// <summary>Solo <see cref="Norvia.Domain.Enums.PropositoVerificacionCorreo.ActivacionCuenta"/>: el Acceso que se activa.</summary>
    public Guid? AccesoId { get; set; }

    /// <summary>Hash del código vigente; el código en claro nunca se guarda.</summary>
    public string CodigoHash { get; set; } = string.Empty;

    /// <summary>Intentos fallidos contra el código vigente (RN-86). Se reinicia al reenviar un código nuevo.</summary>
    public short Intentos { get; set; }
    public DateTime ExpiraEn { get; set; }

    /// <summary>Envíos dentro de la ventana actual (RN-86), para no usar la API como fábrica de correos ajenos.</summary>
    public short EnviosEnVentana { get; set; } = 1;
    public DateTime VentanaEnviosExpiraEn { get; set; }
    public DateTime CreadoEn { get; set; }

    public Acceso? Acceso { get; set; }
}
