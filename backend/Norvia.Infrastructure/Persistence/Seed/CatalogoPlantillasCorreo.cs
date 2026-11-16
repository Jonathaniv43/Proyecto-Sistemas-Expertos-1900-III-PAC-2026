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
            Descripcion: "Se envía al representante tras POST api/empresas con credenciales temporales.",
            Asunto: "Bienvenido a Norvia — acceso a tu empresa",
            CuerpoHtml: """
                <!DOCTYPE html>
                <html lang="es">
                <head><meta charset="utf-8" /><title>Bienvenido a Norvia</title></head>
                <body style="font-family: system-ui, sans-serif; line-height: 1.5; color: #1a1a1a; max-width: 520px;">
                  <h2 style="color: #4f46e5;">Tu empresa está registrada</h2>
                  <p>Hola <strong>{{NombreRepresentante}}</strong>,</p>
                  <p>La empresa <strong>{{RazonSocial}}</strong> (RTN {{RTN}}) fue dada de alta en Norvia.</p>
                  <p>Usá estas credenciales para iniciar sesión. Deberás cambiar la contraseña en el primer acceso.</p>
                  <ul>
                    <li>Correo: <strong>{{Email}}</strong></li>
                    <li>Contraseña temporal: <strong>{{PasswordTemporal}}</strong></li>
                    <li>Identificador de empresa: <code>{{IdEmpresa}}</code></li>
                  </ul>
                  <p><a href="{{UrlLogin}}" style="color: #4f46e5;">Ir al inicio de sesión</a></p>
                  <p style="font-size: 0.875rem; color: #666;">Si no solicitaste este registro, ignorá este mensaje.</p>
                </body>
                </html>
                """),
        new(
            Id: 2,
            Codigo: "cambio_contrasena",
            Nombre: "Solicitud de cambio de contraseña",
            Descripcion: "Flujo de olvidé mi contraseña o cambio obligatorio tras contraseña temporal (pendiente de endpoint).",
            Asunto: "Norvia — cambiá tu contraseña",
            CuerpoHtml: """
                <!DOCTYPE html>
                <html lang="es">
                <head><meta charset="utf-8" /><title>Cambio de contraseña</title></head>
                <body style="font-family: system-ui, sans-serif; line-height: 1.5; color: #1a1a1a; max-width: 520px;">
                  <p>Hola <strong>{{Nombre}}</strong>,</p>
                  <p>Recibimos una solicitud para cambiar la contraseña de tu cuenta Norvia.</p>
                  <p><a href="{{UrlCambio}}" style="color: #4f46e5;">Establecer nueva contraseña</a></p>
                  <p style="font-size: 0.875rem; color: #666;">Si no fuiste vos, ignorá este mensaje.</p>
                </body>
                </html>
                """)
    ];
}