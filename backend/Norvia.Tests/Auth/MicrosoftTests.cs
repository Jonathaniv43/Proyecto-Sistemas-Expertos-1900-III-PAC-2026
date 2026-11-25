using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Auth;

/// <summary>Registro e inicio de sesión con Microsoft (D29, HU-119, HU-120, RN-81 a RN-85). Microsoft es un doble de prueba.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class MicrosoftTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);
    private const string Verificador = "verificador-pkce-de-prueba-con-mas-de-cuarenta-y-tres-caracteres";

    [Fact]
    public async Task Correo_sin_cuenta_registra_la_empresa_sin_contrasena_y_permite_entrar()
    {
        var email = CorreoUnico();
        var identidad = await IdentidadAsync(api.Microsoft.Registrar(email, "ms-" + Guid.NewGuid()));

        Assert.Equal("sin_cuenta", identidad.GetProperty("estado").GetString());
        Assert.Equal(email, identidad.GetProperty("email").GetString());
        Assert.Equal("Mariana", identidad.GetProperty("primerNombre").GetString());

        // RN-86: sin confirmar el correo con el código no se crea la cuenta.
        var sinCodigo = await RegistrarAsync(identidad.GetProperty("tokenIdentidad").GetString()!);
        Assert.Equal(HttpStatusCode.Forbidden, sinCodigo.StatusCode);
        Assert.Equal("auth.correo_sin_verificar", (await Escenario.Json(sinCodigo)).GetProperty("codigo").GetString());

        var tokenConfirmado = await ConfirmarCorreoAsync(identidad.GetProperty("tokenIdentidad").GetString()!, email);
        var registro = await RegistrarAsync(tokenConfirmado, primerNombre: "María");
        Assert.Equal(HttpStatusCode.Created, registro.StatusCode);
        var empresa = await Escenario.Json(registro);
        var idEmpresa = empresa.GetProperty("idEmpresa").GetString()!;

        // RN-81 y RN-84: cuenta sin contraseña, verificada, con el nombre que la persona corrigió.
        await using (var db = api.NuevoContexto(empresaId: null))
        {
            var acceso = await db.Acceso.Include(a => a.Proveedores).SingleAsync(a => a.Email == email);
            Assert.Null(acceso.PasswordHash);
            Assert.True(acceso.Verificado);
            Assert.Single(acceso.Proveedores);
            var persona = await db.VinculoEmpresa.IgnoreQueryFilters()
                .Where(v => v.AccesoId == acceso.Id).Select(v => v.Persona).SingleAsync();
            Assert.Equal("María", persona.PrimerNombre);
        }

        // Sin contraseña no entra por el login clásico, pero sí con Microsoft.
        Assert.Equal(HttpStatusCode.Unauthorized, (await _escenario.LoginEmpresaAsync(email, "Cualquiera#2026x", idEmpresa)).StatusCode);

        var login = await api.CreateClient().PostAsJsonAsync("/api/auth/login/empresa/microsoft",
            new { tokenIdentidad = tokenConfirmado, idEmpresa });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var sesion = await Escenario.Json(login);
        Assert.False(sesion.GetProperty("debeCambiarPassword").GetBoolean());
    }

    [Fact]
    public async Task Correo_con_cuenta_de_contrasena_exige_confirmarla_antes_de_vincular()
    {
        var empresa = await _escenario.CrearEmpresaAsync("MsVincula");
        var userId = "ms-" + Guid.NewGuid();
        var identidad = await IdentidadAsync(api.Microsoft.Registrar(empresa.AdminEmail, userId));
        Assert.Equal("cuenta_con_contrasena", identidad.GetProperty("estado").GetString());
        var token = identidad.GetProperty("tokenIdentidad").GetString()!;

        // Registrar con ese correo sin vincular: conflicto (RN-83).
        Assert.Equal(HttpStatusCode.Conflict, (await RegistrarAsync(token)).StatusCode);

        // Sin vincular, Microsoft no abre sesión (mensaje genérico).
        var antes = await api.CreateClient().PostAsJsonAsync("/api/auth/login/empresa/microsoft",
            new { tokenIdentidad = token, idEmpresa = empresa.IdEmpresaCifrado });
        Assert.Equal(HttpStatusCode.Unauthorized, antes.StatusCode);

        var mala = await api.CreateClient().PostAsJsonAsync("/api/auth/microsoft/vincular",
            new { tokenIdentidad = token, password = "Incorrecta#2026x" });
        Assert.Equal(HttpStatusCode.Unauthorized, mala.StatusCode);

        var ok = await api.CreateClient().PostAsJsonAsync("/api/auth/microsoft/vincular",
            new { tokenIdentidad = token, password = Escenario.PasswordAdmin });
        Assert.Equal(HttpStatusCode.NoContent, ok.StatusCode);

        // Ahora entra con las dos formas.
        Assert.Equal(HttpStatusCode.OK, (await _escenario.LoginEmpresaAsync(empresa.AdminEmail, Escenario.PasswordAdmin, empresa.IdEmpresaCifrado)).StatusCode);
        var login = await api.CreateClient().PostAsJsonAsync("/api/auth/login/empresa/microsoft",
            new { tokenIdentidad = token, idEmpresa = empresa.IdEmpresaCifrado });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        // RN-82: esa cuenta de Microsoft ya no se puede vincular otra vez ni a otra cuenta.
        var otra = await _escenario.CrearEmpresaAsync("MsOtra");
        var otraIdentidad = await IdentidadAsync(api.Microsoft.Registrar(otra.AdminEmail, userId));
        Assert.Equal("cuenta_vinculada", otraIdentidad.GetProperty("estado").GetString());
        var repetida = await api.CreateClient().PostAsJsonAsync("/api/auth/microsoft/vincular",
            new { tokenIdentidad = otraIdentidad.GetProperty("tokenIdentidad").GetString(), password = Escenario.PasswordAdmin });
        Assert.Equal(HttpStatusCode.Conflict, repetida.StatusCode);
    }

    [Fact]
    public async Task Cuenta_vinculada_registra_otra_empresa_sin_pisar_los_datos_de_la_persona()
    {
        var empresa = await _escenario.CrearEmpresaAsync("MsReuso");
        var userId = "ms-" + Guid.NewGuid();
        var token = (await IdentidadAsync(api.Microsoft.Registrar(empresa.AdminEmail, userId))).GetProperty("tokenIdentidad").GetString()!;
        Assert.Equal(HttpStatusCode.NoContent, (await api.CreateClient().PostAsJsonAsync("/api/auth/microsoft/vincular",
            new { tokenIdentidad = token, password = Escenario.PasswordAdmin })).StatusCode);

        // Ahora la identidad vuelve con los nombres guardados (RN-85), no con los de Microsoft.
        var identidad = await IdentidadAsync(api.Microsoft.Registrar(empresa.AdminEmail, userId, nombre: "Otro", apellido: "Nombre"));
        Assert.Equal("cuenta_vinculada", identidad.GetProperty("estado").GetString());
        Assert.True(identidad.GetProperty("personaExistente").GetBoolean());
        Assert.Equal("Admin", identidad.GetProperty("primerNombre").GetString());

        var dni = string.Concat(Enumerable.Range(0, 13).Select(_ => Random.Shared.Next(10)));
        var registro = await RegistrarAsync(identidad.GetProperty("tokenIdentidad").GetString()!, primerNombre: "Intento", tipoDocumento: "dni", numeroDocumento: dni);
        Assert.Equal(HttpStatusCode.Created, registro.StatusCode);

        await using var db = api.NuevoContexto(empresaId: null);
        var acceso = await db.Acceso.SingleAsync(a => a.Email == empresa.AdminEmail);
        var vinculos = await db.VinculoEmpresa.IgnoreQueryFilters().Include(v => v.Persona).Where(v => v.AccesoId == acceso.Id).ToListAsync();
        Assert.Equal(2, vinculos.Count);
        Assert.Single(vinculos.Select(v => v.PersonaId).Distinct());
        var persona = vinculos[0].Persona;
        Assert.Equal("Admin", persona.PrimerNombre);       // no se sobrescribe
        Assert.Equal(dni, persona.NumeroDocumento);         // solo se completa lo vacío
    }

    [Fact]
    public async Task Codigo_incorrecto_se_rechaza_y_tras_cinco_intentos_se_bloquea()
    {
        var email = CorreoUnico();
        var token = (await IdentidadAsync(api.Microsoft.Registrar(email, "ms-" + Guid.NewGuid()))).GetProperty("tokenIdentidad").GetString()!;
        Assert.Equal(HttpStatusCode.OK, (await EnviarCodigoAsync(token)).StatusCode);

        for (var i = 0; i < 5; i++)
        {
            var mal = await VerificarCodigoAsync(token, "000000");
            // Salvo que el código real sea 000000 (1 en un millón): entonces no es un intento fallido.
            Assert.True(mal.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.OK);
        }

        // El sexto intento se frena aunque el código fuera el correcto.
        var real = CodigoRecibido(email);
        Assert.Equal((HttpStatusCode)429, (await VerificarCodigoAsync(token, real)).StatusCode);
    }

    [Fact]
    public async Task Codigo_no_se_envia_si_el_correo_ya_tiene_cuenta_y_se_limita_el_reenvio()
    {
        var empresa = await _escenario.CrearEmpresaAsync("MsSinCodigo");
        var existente = (await IdentidadAsync(api.Microsoft.Registrar(empresa.AdminEmail, "ms-" + Guid.NewGuid()))).GetProperty("tokenIdentidad").GetString()!;
        Assert.Equal(HttpStatusCode.Conflict, (await EnviarCodigoAsync(existente)).StatusCode);

        var token = (await IdentidadAsync(api.Microsoft.Registrar(CorreoUnico(), "ms-" + Guid.NewGuid()))).GetProperty("tokenIdentidad").GetString()!;
        for (var i = 0; i < 3; i++) Assert.Equal(HttpStatusCode.OK, (await EnviarCodigoAsync(token)).StatusCode);
        Assert.Equal((HttpStatusCode)429, (await EnviarCodigoAsync(token)).StatusCode);
    }

    [Fact]
    public async Task Token_de_identidad_alterado_o_codigo_desconocido_se_rechaza()
    {
        var codigo = await api.CreateClient().PostAsJsonAsync("/api/auth/microsoft/identidad",
            new { codigo = "codigo-que-no-existe", codeVerifier = Verificador, redirectUri = "http://localhost:5173/auth/microsoft/callback" });
        Assert.Equal(HttpStatusCode.Unauthorized, codigo.StatusCode);

        var token = (await IdentidadAsync(api.Microsoft.Registrar(CorreoUnico(), "ms-" + Guid.NewGuid()))).GetProperty("tokenIdentidad").GetString()!;
        var alterado = token[..^3] + (token[^3] == 'A' ? "BBB" : "AAA");
        Assert.Equal(HttpStatusCode.Unauthorized, (await RegistrarAsync(alterado)).StatusCode);

        var login = await api.CreateClient().PostAsJsonAsync("/api/auth/login/empresa/microsoft",
            new { tokenIdentidad = alterado, idEmpresa = "x" });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    private Task<HttpResponseMessage> EnviarCodigoAsync(string token) =>
        api.CreateClient().PostAsJsonAsync("/api/auth/microsoft/codigo", new { tokenIdentidad = token });

    private Task<HttpResponseMessage> VerificarCodigoAsync(string token, string codigo) =>
        api.CreateClient().PostAsJsonAsync("/api/auth/microsoft/codigo/verificar", new { tokenIdentidad = token, codigo });

    /// <summary>Último código de 6 dígitos que la API "envió" a ese correo.</summary>
    private string CodigoRecibido(string email) =>
        System.Text.RegularExpressions.Regex.Match(api.Correos.Para(email).Last().CuerpoHtml, @">(\d{6})<").Groups[1].Value;

    /// <summary>Pide el código, comprueba que uno incorrecto se rechaza y confirma con el correcto: devuelve el token ya confirmado.</summary>
    private async Task<string> ConfirmarCorreoAsync(string token, string email)
    {
        Assert.Equal(HttpStatusCode.OK, (await EnviarCodigoAsync(token)).StatusCode);
        var codigo = CodigoRecibido(email);
        Assert.Matches(@"^\d{6}$", codigo);

        var incorrecto = codigo == "123456" ? "654321" : "123456";
        var mal = await VerificarCodigoAsync(token, incorrecto);
        Assert.Equal(HttpStatusCode.BadRequest, mal.StatusCode);
        Assert.Equal("auth.codigo_incorrecto", (await Escenario.Json(mal)).GetProperty("codigo").GetString());

        var bien = await VerificarCodigoAsync(token, codigo);
        Assert.Equal(HttpStatusCode.OK, bien.StatusCode);
        return (await Escenario.Json(bien)).GetProperty("tokenIdentidad").GetString()!;
    }

    private async Task<System.Text.Json.JsonElement> IdentidadAsync(string codigo)
    {
        var r = await api.CreateClient().PostAsJsonAsync("/api/auth/microsoft/identidad",
            new { codigo, codeVerifier = Verificador, redirectUri = "http://localhost:5173/auth/microsoft/callback" });
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        return await Escenario.Json(r);
    }

    private async Task<HttpResponseMessage> RegistrarAsync(
        string tokenIdentidad, string primerNombre = "Mariana", string? tipoDocumento = null, string? numeroDocumento = null) =>
        await api.CreateClient().PostAsJsonAsync("/api/empresas/microsoft", new
        {
            tokenIdentidad,
            razonSocial = "Empresa Microsoft S. de R.L.",
            rtn = string.Concat(Enumerable.Range(0, 14).Select(_ => Random.Shared.Next(10))),
            rubroCodigo = "comercio",
            primerNombre,
            primerApellido = "Lopez",
            tipoDocumento,
            numeroDocumento
        });

    private static string CorreoUnico() => $"ms.{Guid.NewGuid():N}@pruebas.hn";
}
