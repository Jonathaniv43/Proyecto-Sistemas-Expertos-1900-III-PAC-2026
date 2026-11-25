namespace Norvia.Domain.Entities.Acceso;

/// <summary>
/// Código de 6 dígitos para confirmar que un correo de Microsoft es de quien se registra, antes de crear su
/// cuenta (D29, RN-86). Global, sin EmpresaId: todavía no hay empresa ni Acceso. Una sola fila por correo y
/// cuenta del proveedor; reenviar el código actualiza la misma fila en vez de acumular filas.
/// </summary>
public class VerificacionCorreoMicrosoft
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Email { get; set; } = string.Empty;
    public string ProveedorUserId { get; set; } = string.Empty;

    /// <summary>Hash del código vigente; el código en claro nunca se guarda.</summary>
    public string CodigoHash { get; set; } = string.Empty;

    /// <summary>Intentos fallidos contra el código vigente (RN-86). Se reinicia al reenviar un código nuevo.</summary>
    public short Intentos { get; set; }
    public DateTime ExpiraEn { get; set; }

    /// <summary>Envíos dentro de la ventana actual (RN-86), para no usar la API como fábrica de correos ajenos.</summary>
    public short EnviosEnVentana { get; set; } = 1;
    public DateTime VentanaEnviosExpiraEn { get; set; }
    public DateTime CreadoEn { get; set; }
}
