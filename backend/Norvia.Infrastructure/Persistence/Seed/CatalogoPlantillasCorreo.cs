namespace Norvia.Infrastructure.Persistence.Seed;

/// <summary>Contenido inicial de plantillas de correo; <see cref="PlantillasCorreoSeeder"/> lo sincroniza a la BD.</summary>
public sealed record PlantillaCorreoDef(
    short Id, string Codigo, string Nombre, string Descripcion, string Asunto, string CuerpoHtml);

public static class CatalogoPlantillasCorreo
{
    public static readonly PlantillaCorreoDef[] Todas =
    [
        new(
            Id: 1,
            Codigo: "bienvenida_empresa",
            Nombre: "Bienvenida al registrar empresa",
            Descripcion: "Se envía al representante tras POST api/empresas con credenciales temporales y enlace de verificación.",
            Asunto: "Bienvenido a Norvia — verificá tu cuenta",
            CuerpoHtml: """
                <!DOCTYPE html>
                <html lang="es">
                <head><meta charset="utf-8" /><title>Bienvenido a Norvia</title></head>
                <body style="font-family: system-ui, sans-serif; line-height: 1.5; color: #1a1a1a; max-width: 520px;">
                  <h2 style="color: #4f46e5;">Tu empresa está registrada</h2>
                  <p>Hola <strong>{{NombreRepresentante}}</strong>,</p>
                  <p>La empresa <strong>{{RazonSocial}}</strong> (RTN {{RTN}}) fue dada de alta en Norvia.</p>
                  <p>Antes de iniciar sesión debés <strong>verificar tu cuenta</strong> con el correo, la contraseña temporal y una contraseña nueva.</p>
                  <ul>
                    <li>Correo: <strong>{{Email}}</strong></li>
                    <li>Contraseña temporal: <strong>{{PasswordTemporal}}</strong></li>
                    <li>Identificador de empresa: <code>{{IdEmpresa}}</code></li>
                  </ul>
                  <p><a href="{{UrlVerificacion}}" style="color: #4f46e5;">Verificar mi cuenta</a></p>
                  <p>Después de verificar, podés <a href="{{UrlLogin}}" style="color: #4f46e5;">iniciar sesión</a>.</p>
                  <p style="font-size: 0.875rem; color: #666;">Si no solicitaste este registro, ignorá este mensaje.</p>
                </body>
                </html>
                """),
        new(
            Id: 2,
            Codigo: "cambio_contrasena",
            Nombre: "Solicitud de cambio de contraseña",
            Descripcion: "HU-022: enlace para cambiar la contraseña (POST api/auth/contrasena/solicitud). El enlace vence a los 30 minutos y sirve una sola vez.",
            Asunto: "Norvia — cambiá tu contraseña",
            CuerpoHtml: """
                <!DOCTYPE html>
                <html lang="es">
                <head><meta charset="utf-8" /><title>Cambio de contraseña</title></head>
                <body style="font-family: system-ui, sans-serif; line-height: 1.5; color: #1a1a1a; max-width: 520px;">
                  <p>Hola <strong>{{Nombre}}</strong>,</p>
                  <p>Recibimos una solicitud para cambiar la contraseña de tu cuenta Norvia.</p>
                  <p><a href="{{UrlCambio}}" style="color: #4f46e5;">Establecer nueva contraseña</a></p>
                  <p>El enlace vence en {{Minutos}} minutos y sirve una sola vez. La contraseña nueva valdrá en todas las empresas donde tenés acceso, y se cerrarán las sesiones abiertas.</p>
                  <p style="font-size: 0.875rem; color: #666;">Si no fuiste vos, ignorá este mensaje.</p>
                </body>
                </html>
                """),
        new(
            Id: 3,
            Codigo: "reenvio_verificacion",
            Nombre: "Reenvío de verificación de cuenta",
            Descripcion: "Recordatorio para verificar la cuenta; no incluye la temporal (solo el hash está en BD).",
            Asunto: "Norvia — verificá tu cuenta",
            CuerpoHtml: """
                <!DOCTYPE html>
                <html lang="es">
                <head><meta charset="utf-8" /><title>Verificar cuenta</title></head>
                <body style="font-family: system-ui, sans-serif; line-height: 1.5; color: #1a1a1a; max-width: 520px;">
                  <p>Hola,</p>
                  <p>Recibimos una solicitud para reenviar las instrucciones de verificación de <strong>{{Email}}</strong>.</p>
                  <p>Usá la contraseña temporal del correo de bienvenida original y elegí tu contraseña nueva en el formulario.</p>
                  <p><a href="{{UrlVerificacion}}" style="color: #4f46e5;">Verificar mi cuenta</a></p>
                  <p style="font-size: 0.875rem; color: #666;">Si no solicitaste este mensaje, ignorálo.</p>
                </body>
                </html>
                """),
        new(
            Id: 4,
            Codigo: "notificacion",
            Nombre: "Notificación de Norvia",
            Descripcion: "HU-118: correo de una notificación (tarea asignada, HU-081; política publicada, HU-116), enviado por la bandeja de salida con reintentos. Los valores llegan ya codificados para HTML, salvo {{Asunto}}.",
            Asunto: "Norvia — {{Asunto}}",
            CuerpoHtml: """
                <!DOCTYPE html>
                <html lang="es">
                <head><meta charset="utf-8" /><title>{{Titulo}}</title></head>
                <body style="font-family: system-ui, sans-serif; line-height: 1.5; color: #1a1a1a; max-width: 520px;">
                  <p>Hola <strong>{{Nombre}}</strong>,</p>
                  <h2 style="font-size: 1.125rem;">{{Titulo}}</h2>
                  <p>{{Mensaje}}</p>
                  <p><a href="{{Url}}" style="color: #4f46e5;">Abrir en Norvia</a></p>
                  <p style="font-size: 0.875rem; color: #666;">También la encontrás en tus notificaciones dentro de Norvia.</p>
                </body>
                </html>
                """)
        ,
        new(
            Id: 5,
            Codigo: "codigo_verificacion_correo",
            Nombre: "Código para confirmar el correo",
            Descripcion: "D29, RN-86: código de 6 dígitos al registrar una empresa con una cuenta de Microsoft que aún no tiene cuenta en Norvia. Vence en unos 10 minutos.",
            Asunto: "Norvia — tu código de verificación",
            CuerpoHtml: """
                <!DOCTYPE html>
                <html lang="es">
                <head><meta charset="utf-8" /><title>Código de verificación</title></head>
                <body style="font-family: system-ui, sans-serif; line-height: 1.5; color: #1a1a1a; max-width: 520px;">
                  <p>Hola,</p>
                  <p>Usa este código para confirmar que <strong>{{Email}}</strong> es tu correo y terminar el registro en Norvia:</p>
                  <p style="font-size: 1.75rem; letter-spacing: 0.35em; font-weight: 600;">{{Codigo}}</p>
                  <p>El código vence en unos {{Minutos}} minutos.</p>
                  <p style="font-size: 0.875rem; color: #666;">Si no iniciaste este registro, ignora este mensaje: nadie podrá crear una cuenta con tu correo sin este código.</p>
                </body>
                </html>
                """)
    ];
}