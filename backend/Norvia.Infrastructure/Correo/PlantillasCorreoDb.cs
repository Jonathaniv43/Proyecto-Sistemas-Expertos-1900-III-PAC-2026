using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Norvia.Application.Common.Correo;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Correo;

public sealed partial class PlantillasCorreoDb(NorviaDbContext db) : IPlantillasCorreo
{
    private static readonly Regex Marcador = MarcadorRegex();

    public async Task<CorreoRenderizado?> RenderizarAsync(
        string codigoPlantilla,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken cancellationToken)
    {
        var plantilla = await db.PlantillaCorreo
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Codigo == codigoPlantilla && p.Activo, cancellationToken);

        if (plantilla is null)
            return null;

        var html = Marcador.Replace(plantilla.CuerpoHtml, m =>
            variables.TryGetValue(m.Groups[1].Value, out var valor) ? valor : m.Value);

        var asunto = Marcador.Replace(plantilla.Asunto, m =>
            variables.TryGetValue(m.Groups[1].Value, out var valor) ? valor : m.Value);

        return new CorreoRenderizado(asunto, html);
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}", RegexOptions.Compiled)]
    private static partial Regex MarcadorRegex();
}