namespace Norvia.Application.Common.Correo;

public interface IPlantillasCorreo
{
    /// <summary>Carga la plantilla activa por código y reemplaza marcadores {{Nombre}}.</summary>
    Task<CorreoRenderizado?> RenderizarAsync(
        string codigoPlantilla,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken cancellationToken);
}