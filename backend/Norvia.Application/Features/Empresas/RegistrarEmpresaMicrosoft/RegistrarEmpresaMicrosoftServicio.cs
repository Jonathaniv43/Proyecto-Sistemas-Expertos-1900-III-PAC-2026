using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Empresas.RegistrarEmpresa.DTOs;
using Norvia.Application.Features.Empresas.RegistrarEmpresaMicrosoft.DTOs;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Empresas.RegistrarEmpresaMicrosoft;

/// <summary>
/// Registra una empresa con una cuenta de Microsoft (D29, HU-119): sin contraseña ni correo temporal, la cuenta
/// nace verificada. Si esa cuenta de Microsoft ya está vinculada a un Acceso, se reutilizan su Persona y su
/// cuenta, y solo se crean la empresa y el vínculo de Administrador. Los datos ya guardados de la Persona no se
/// sobrescriben: solo se completan los vacíos (RN-85).
/// </summary>
public sealed class RegistrarEmpresaMicrosoftServicio(
    IRegistrarEmpresaMicrosoftRepositorio repo,
    IUnidadDeTrabajo uow,
    ITokenIdentidadExterna tokensIdentidad,
    ICifradorEmpresaId cifrador,
    IReloj reloj) : IServicioCasoDeUso
{
    public async Task<Resultado<RegistrarEmpresaResponseDto>> EjecutarAsync(
        RegistrarEmpresaMicrosoftRequestDto request, CancellationToken cancellationToken)
    {
        var ahora = await reloj.AhoraAsync(cancellationToken);
        var identidad = tokensIdentidad.Leer(request.TokenIdentidad, ahora);
        if (identidad is null) return Auth.Errores.IdentidadInvalida;

        var rtn = request.RTN.Trim();
        if (await repo.ExisteRtnAsync(rtn, cancellationToken)) return Errores.RtnDuplicado;

        var rubroId = await repo.BuscarRubroIdActivoAsync(request.RubroCodigo.Trim().ToLowerInvariant(), cancellationToken);
        if (rubroId is null) return Errores.RubroInvalido;

        TipoDocumentoIdentidad? tipoDocumento = request.TipoDocumento?.Trim().ToLowerInvariant() switch
        {
            "dni" => TipoDocumentoIdentidad.Dni,
            "pasaporte" => TipoDocumentoIdentidad.Pasaporte,
            _ => null
        };
        var numeroDocumento = string.IsNullOrWhiteSpace(request.NumeroDocumento) ? null : request.NumeroDocumento.Trim();

        var empresa = new Empresa
        {
            RazonSocial = request.RazonSocial.Trim(),
            RTN = rtn,
            RubroId = rubroId,
            Estado = EstadoEmpresa.Activa
        };

        var acceso = await repo.BuscarAccesoPorProveedorAsync(identidad.Proveedor, identidad.ProveedorUserId, cancellationToken);
        VinculoEmpresa vinculo;
        string email;

        if (acceso is not null)
        {
            var persona = await repo.BuscarPersonaDeAccesoAsync(acceso.Id, cancellationToken);
            if (persona is null || !await repo.UsuarioActivoAsync(persona.Id, cancellationToken))
                return Errores.CuentaNoDisponible;

            // RN-85: nunca se pisa lo guardado; solo se completa el documento si estaba vacío.
            if (persona.TipoDocumento is null && persona.NumeroDocumento is null && tipoDocumento is not null && numeroDocumento is not null)
            {
                if (await repo.ExisteDocumentoAsync(tipoDocumento.Value, numeroDocumento, cancellationToken))
                    return Errores.DocumentoDuplicado;
                persona.TipoDocumento = tipoDocumento;
                persona.NumeroDocumento = numeroDocumento;
            }

            vinculo = NuevoVinculo(empresa, persona.Id, acceso.Id);
            repo.AgregarEmpresaYVinculo(empresa, vinculo);
            email = acceso.Email;
        }
        else
        {
            // El correo ya tiene cuenta que no es de este Microsoft: debe vincularlo primero (RN-83).
            if (await repo.ExisteEmailAsync(identidad.Email, cancellationToken)) return Errores.EmailDuplicado;

            // Microsoft no garantiza que el correo sea de quien entra: crear una cuenta exige el código de 6 dígitos (RN-86).
            if (!identidad.CorreoVerificado) return Auth.Errores.CorreoSinVerificar;

            if (tipoDocumento is not null && numeroDocumento is not null
                && await repo.ExisteDocumentoAsync(tipoDocumento.Value, numeroDocumento, cancellationToken))
                return Errores.DocumentoDuplicado;

            var persona = new Persona
            {
                PrimerNombre = request.PrimerNombre.Trim(),
                SegundoNombre = Opcional(request.SegundoNombre),
                PrimerApellido = request.PrimerApellido.Trim(),
                SegundoApellido = Opcional(request.SegundoApellido),
                TipoDocumento = numeroDocumento is null ? null : tipoDocumento,
                NumeroDocumento = numeroDocumento
            };
            var usuario = new Usuario { PersonaId = persona.Id, Activo = true };
            // RN-81 y RN-84: sin contraseña, verificada porque el proveedor ya confirmó el correo.
            acceso = new Acceso { Email = identidad.Email, Verificado = true, DebeCambiarPassword = false };
            var proveedor = new AccesoProveedor
            {
                AccesoId = acceso.Id,
                Proveedor = identidad.Proveedor,
                ProveedorUserId = identidad.ProveedorUserId
            };
            vinculo = NuevoVinculo(empresa, persona.Id, acceso.Id);
            repo.AgregarTodo(empresa, persona, usuario, acceso, proveedor, vinculo);
            email = acceso.Email;
        }

        await uow.GuardarCambiosAsync(cancellationToken);

        return new RegistrarEmpresaResponseDto(
            cifrador.Cifrar(empresa.Id), empresa.RazonSocial, empresa.RTN, vinculo.Id, email, CorreoEnviado: false);
    }

    private static VinculoEmpresa NuevoVinculo(Empresa empresa, Guid personaId, Guid accesoId) => new()
    {
        EmpresaId = empresa.Id,
        PersonaId = personaId,
        AccesoId = accesoId,
        RolId = Rol.AdminEmpresaId,
        Estado = EstadoVinculo.Activo
    };

    private static string? Opcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
