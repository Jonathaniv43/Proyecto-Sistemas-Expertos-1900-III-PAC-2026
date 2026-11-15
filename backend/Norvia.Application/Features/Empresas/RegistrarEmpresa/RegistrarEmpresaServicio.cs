using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Empresas.RegistrarEmpresa;

/// <summary>Registra una empresa nueva y otorga a su representante el primer vínculo como Administrador.</summary>
public sealed class RegistrarEmpresaServicio(
    IRegistrarEmpresaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IHasherPassword hasherPassword,
    ICifradorEmpresaId cifrador) : IServicioCasoDeUso
{
    public async Task<Resultado<RegistrarEmpresaResponse>> EjecutarAsync(
        RegistrarEmpresaRequest request, CancellationToken cancellationToken)
    {
        var rtn = request.RTN.Trim();
        if (await repositorio.ExisteRtnAsync(rtn, cancellationToken)) return Errores.RtnDuplicado;

        var email = request.Email.Trim().ToLowerInvariant();
        if (await repositorio.ExisteEmailAsync(email, cancellationToken)) return Errores.EmailDuplicado;

        var empresa = new Empresa
        {
            RazonSocial = request.RazonSocial.Trim(),
            RTN = rtn,
            Estado = EstadoEmpresa.Activa
        };

        var persona = new Persona
        {
            PrimerNombre = request.PrimerNombre.Trim(),
            SegundoNombre = string.IsNullOrWhiteSpace(request.SegundoNombre) ? null : request.SegundoNombre.Trim(),
            PrimerApellido = request.PrimerApellido.Trim(),
            SegundoApellido = string.IsNullOrWhiteSpace(request.SegundoApellido) ? null : request.SegundoApellido.Trim()
        };

        var usuario = new Usuario
        {
            PersonaId = persona.Id,
            Activo = true
        };

        var acceso = new Acceso
        {
            Email = email,
            Verificado = true,
            DebeCambiarPassword = false
        };
        acceso.PasswordHash = hasherPassword.Hashear(acceso, request.Password);

        var vinculo = new VinculoEmpresa
        {
            EmpresaId = empresa.Id,
            PersonaId = persona.Id,
            AccesoId = acceso.Id,
            RolId = Rol.AdminEmpresaId, //Revisar esto
            Estado = EstadoVinculo.Activo
        };
        
        repositorio.Agregar(empresa, persona, usuario, acceso, vinculo);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return new RegistrarEmpresaResponse(
            cifrador.Cifrar(empresa.Id),
            empresa.RazonSocial,
            empresa.RTN,
            vinculo.Id,
            acceso.Email);
    }
}