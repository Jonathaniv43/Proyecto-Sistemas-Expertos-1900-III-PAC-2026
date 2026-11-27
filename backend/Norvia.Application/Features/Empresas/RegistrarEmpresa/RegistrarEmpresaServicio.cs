using Microsoft.Extensions.Options;
using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Correo;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Common.Seguridad;
using Norvia.Application.Features.Empresas.RegistrarEmpresa.DTOs;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Empresas.RegistrarEmpresa;

/// <summary>Registra una empresa nueva y otorga a su representante el primer vínculo como Administrador.</summary>
public sealed class RegistrarEmpresaServicio(
    IRegistrarEmpresaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IHashCodigoVerificacion hashCodigo,
    IReloj reloj,
    ICifradorEmpresaId cifrador,
    IPlantillasCorreo plantillas,
    IEnvioCorreo envioCorreo,
    IOptions<CorreoOpciones> correoOpciones) : IServicioCasoDeUso
{
    public async Task<Resultado<RegistrarEmpresaResponseDto>> EjecutarAsync(
        RegistrarEmpresaRequestDto request, CancellationToken cancellationToken)
    {
        var rtn = request.RTN.Trim();
        if (await repositorio.ExisteRtnAsync(rtn, cancellationToken)) return Errores.RtnDuplicado;

        var email = request.Email.Trim().ToLowerInvariant();
        if (await repositorio.ExisteEmailAsync(email, cancellationToken)) return Errores.EmailDuplicado;

        var tipoDocumento = request.TipoDocumento.Trim().ToLowerInvariant();
        if (!await repositorio.TipoDocumentoIdentidadExisteAsync(tipoDocumento, cancellationToken))
            return Errores.TipoDocumentoInvalido;
        var tipoEnum = tipoDocumento switch
        {
            "dni" => TipoDocumentoIdentidad.Dni,
            "pasaporte" => TipoDocumentoIdentidad.Pasaporte,
            "otro" => TipoDocumentoIdentidad.Otro,
            _ => default
        };

        var numeroDocumento = request.NumeroDocumento.Trim();
        if (await repositorio.ExisteDocumentoAsync(tipoEnum, numeroDocumento, cancellationToken))
            return Errores.DocumentoDuplicado;

        var rubroCodigo = request.RubroCodigo.Trim().ToLowerInvariant();
        var rubroId = await repositorio.BuscarRubroIdActivoAsync(rubroCodigo, cancellationToken);
        if (rubroId is null) return Errores.RubroInvalido;

        var empresa = new Empresa
        {
            RazonSocial = request.RazonSocial.Trim(),
            RTN = rtn,
            RubroId = rubroId,
            Estado = EstadoEmpresa.Activa
        };

        var persona = new Persona
        {
            PrimerNombre = request.PrimerNombre.Trim(),
            SegundoNombre = string.IsNullOrWhiteSpace(request.SegundoNombre) ? null : request.SegundoNombre.Trim(),
            PrimerApellido = request.PrimerApellido.Trim(),
            SegundoApellido = string.IsNullOrWhiteSpace(request.SegundoApellido) ? null : request.SegundoApellido.Trim(),
            TipoDocumento = tipoEnum,
            NumeroDocumento = numeroDocumento
        };

        var usuario = new Usuario
        {
            PersonaId = persona.Id,
            Activo = true
        };

        var acceso = new Acceso
        {
            Email = email,
            Verificado = false
        };

        var vinculo = new VinculoEmpresa
        {
            EmpresaId = empresa.Id,
            PersonaId = persona.Id,
            AccesoId = acceso.Id,
            RolId = Rol.AdminEmpresaId,
            Estado = EstadoVinculo.Activo
        };

        var ahora = await reloj.AhoraAsync(cancellationToken);
        var codigo = hashCodigo.Generar();
        var verificacion = new VerificacionCorreo
        {
            Proposito = PropositoVerificacionCorreo.ActivacionCuenta,
            Email = acceso.Email,
            AccesoId = acceso.Id,
            CodigoHash = hashCodigo.Hashear(codigo),
            ExpiraEn = ahora.AddMinutes(VentanaEnvioCodigo.MinutosVigencia),
            VentanaEnviosExpiraEn = ahora.AddMinutes(VentanaEnvioCodigo.MinutosVigencia)
        };

        repositorio.Agregar(empresa, persona, usuario, acceso, vinculo, verificacion);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        var idEmpresaCifrado = cifrador.Cifrar(empresa.Id);
        var nombreRepresentante = persona.NombreCompleto;
        var urlBase = correoOpciones.Value.UrlBaseFrontend.TrimEnd('/');
        var urlActivacion =
            $"{urlBase}/verificar-cuenta?correo={Uri.EscapeDataString(acceso.Email)}&empresa={Uri.EscapeDataString(idEmpresaCifrado)}";

        var variables = new Dictionary<string, string>
        {
            ["NombreRepresentante"] = nombreRepresentante,
            ["EmpresaNombre"] = empresa.RazonSocial,
            ["RolNombre"] = "Administrador",
            ["RazonSocial"] = empresa.RazonSocial,
            ["RTN"] = empresa.RTN,
            ["Email"] = acceso.Email,
            ["Codigo"] = codigo,
            ["Minutos"] = VentanaEnvioCodigo.MinutosVigencia.ToString(),
            ["IdEmpresa"] = idEmpresaCifrado,
            ["UrlLogin"] = urlBase,
            ["UrlActivacion"] = urlActivacion
        };

        var renderizado = await plantillas.RenderizarAsync(
            CodigosPlantillaCorreo.ActivacionCuenta, variables, cancellationToken);
        if (renderizado is null)
            return Errores.PlantillaCorreoNoEncontrada;

        var correoEnviado = (await envioCorreo.EnviarAsync(
            new MensajeCorreo(acceso.Email, renderizado.Asunto, renderizado.CuerpoHtml),
            cancellationToken)).Enviado;

        return new RegistrarEmpresaResponseDto(
            idEmpresaCifrado,
            empresa.RazonSocial,
            empresa.RTN,
            vinculo.Id,
            acceso.Email,
            correoEnviado);
    }
}