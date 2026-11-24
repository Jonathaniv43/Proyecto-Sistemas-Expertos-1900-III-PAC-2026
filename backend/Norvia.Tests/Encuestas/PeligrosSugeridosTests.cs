using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Encuestas;

/// <summary>
/// HU-033: peligros sugeridos a partir de las respuestas (RN-13, RN-14, RN-73). Las encuestas y respuestas se
/// arman directo en la base, para no depender de cómo se crean y responden (HU-030, HU-031).
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class PeligrosSugeridosTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Con_menos_de_3_respuestas_no_muestra_ni_registra_sugerencias()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var e = await CrearEncuestaAsync(empresa.EmpresaId, empresa.AdminVinculoId);
        await ResponderAsync(empresa.EmpresaId, e.VersionId, (e.Unica, e.UnicaSi));
        await ResponderAsync(empresa.EmpresaId, e.VersionId, (e.Unica, e.UnicaSi));

        var r = await Escenario.Json(await cliente.GetAsync(Ruta(e.EncuestaId)));

        Assert.True(r.GetProperty("detalleOculto").GetBoolean());
        Assert.Equal(2, r.GetProperty("totalRespuestas").GetInt32());
        Assert.Empty(r.GetProperty("sugerencias").EnumerateArray());
        Assert.Equal(0, await ContarRiesgosAsync(empresa.EmpresaId, e.EncuestaId));
    }

    [Fact]
    public async Task Sugiere_los_peligros_con_exposicion_y_cuenta_cuantas_respuestas_los_respaldan()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var e = await CrearEncuestaAsync(empresa.EmpresaId, empresa.AdminVinculoId);
        // R1: exposición en la única (peligro X). R2: exposición en la única y en la múltiple (X e Y). R3: ninguna.
        await ResponderAsync(empresa.EmpresaId, e.VersionId, (e.Unica, e.UnicaSi));
        await ResponderAsync(empresa.EmpresaId, e.VersionId, (e.Unica, e.UnicaSi), (e.Multiple, e.MultipleExpone), (e.Multiple, e.MultipleNoExpone));
        await ResponderAsync(empresa.EmpresaId, e.VersionId, (e.Unica, e.UnicaNo), (e.Multiple, e.MultipleNoExpone));

        var r = await Escenario.Json(await cliente.GetAsync(Ruta(e.EncuestaId)));

        Assert.False(r.GetProperty("detalleOculto").GetBoolean());
        var s = Sugerencias(r);
        Assert.Equal(2, s.Count);
        Assert.Equal(2, s[e.PeligroX].GetProperty("respuestasConExposicion").GetInt32());
        Assert.Equal(1, s[e.PeligroY].GetProperty("respuestasConExposicion").GetInt32());
        Assert.Equal("sugerido", s[e.PeligroX].GetProperty("estado").GetString());
        var origen = Assert.Single(s[e.PeligroY].GetProperty("preguntas").EnumerateArray());
        Assert.Equal(e.Multiple, origen.GetProperty("preguntaId").GetGuid());
        Assert.Equal(2, await ContarRiesgosAsync(empresa.EmpresaId, e.EncuestaId));
    }

    [Fact]
    public async Task Las_mismas_respuestas_dan_las_mismas_sugerencias_y_las_nuevas_no_las_repiten()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var e = await CrearEncuestaAsync(empresa.EmpresaId, empresa.AdminVinculoId);
        for (var i = 0; i < 3; i++) await ResponderAsync(empresa.EmpresaId, e.VersionId, (e.Unica, e.UnicaSi));

        var primera = await Escenario.Json(await cliente.GetAsync(Ruta(e.EncuestaId)));
        var segunda = await Escenario.Json(await cliente.GetAsync(Ruta(e.EncuestaId)));
        Assert.Equal(primera.ToString(), segunda.ToString()); // RN-14

        await ResponderAsync(empresa.EmpresaId, e.VersionId, (e.Unica, e.UnicaSi));
        var tercera = Sugerencias(await Escenario.Json(await cliente.GetAsync(Ruta(e.EncuestaId))));

        Assert.Equal(4, tercera[e.PeligroX].GetProperty("respuestasConExposicion").GetInt32());
        Assert.Equal(Sugerencias(primera)[e.PeligroX].GetProperty("riesgoSugeridoId").GetGuid(),
                     tercera[e.PeligroX].GetProperty("riesgoSugeridoId").GetGuid());
        Assert.Equal(1, await ContarRiesgosAsync(empresa.EmpresaId, e.EncuestaId));
    }

    [Fact]
    public async Task Una_version_que_cambia_el_peligro_de_la_pregunta_sugiere_el_peligro_nuevo()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var e = await CrearEncuestaAsync(empresa.EmpresaId, empresa.AdminVinculoId);
        for (var i = 0; i < 3; i++) await ResponderAsync(empresa.EmpresaId, e.VersionId, (e.Unica, e.UnicaSi));

        // Versión 2: la misma pregunta ahora apunta al peligro Z.
        var v2 = await CrearVersionConPeligroAsync(empresa.EmpresaId, empresa.AdminVinculoId, e, nuevoPeligro: e.PeligroZ);
        await ResponderAsync(empresa.EmpresaId, v2.VersionId, (v2.Unica, v2.UnicaSi));

        var s = Sugerencias(await Escenario.Json(await cliente.GetAsync(Ruta(e.EncuestaId))));

        Assert.Equal(3, s[e.PeligroX].GetProperty("respuestasConExposicion").GetInt32()); // respuestas de la versión 1
        Assert.Equal(1, s[e.PeligroZ].GetProperty("respuestasConExposicion").GetInt32()); // respuesta de la versión 2
    }

    [Fact]
    public async Task Una_sugerencia_ya_revisada_conserva_su_estado_y_su_actividad_y_no_se_duplica()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var e = await CrearEncuestaAsync(empresa.EmpresaId, empresa.AdminVinculoId);
        for (var i = 0; i < 3; i++) await ResponderAsync(empresa.EmpresaId, e.VersionId, (e.Unica, e.UnicaSi));
        var antes = Sugerencias(await Escenario.Json(await cliente.GetAsync(Ruta(e.EncuestaId))));
        Assert.Equal(JsonValueKind.Null, antes[e.PeligroX].GetProperty("actividad").ValueKind);

        // HU-035: se confirma con su actividad.
        var riesgoId = antes[e.PeligroX].GetProperty("riesgoSugeridoId").GetGuid();
        var confirmar = await cliente.PatchAsJsonAsync($"{Ruta(e.EncuestaId)}/{riesgoId}",
            new { estado = "confirmado", actividad = "Bodega" });
        Assert.Equal(HttpStatusCode.OK, confirmar.StatusCode);
        await ResponderAsync(empresa.EmpresaId, e.VersionId, (e.Unica, e.UnicaSi));

        var s = Sugerencias(await Escenario.Json(await cliente.GetAsync(Ruta(e.EncuestaId))));

        Assert.Equal("confirmado", s[e.PeligroX].GetProperty("estado").GetString());
        Assert.Equal("Bodega", s[e.PeligroX].GetProperty("actividad").GetString());
        Assert.Equal(1, await ContarRiesgosAsync(empresa.EmpresaId, e.EncuestaId));
    }

    [Fact]
    public async Task Otra_empresa_no_puede_ver_las_sugerencias_de_una_encuesta_ajena()
    {
        var (a, _) = await EmpresaConResponsableAsync();
        var (_, clienteB) = await EmpresaConResponsableAsync();
        var e = await CrearEncuestaAsync(a.EmpresaId, a.AdminVinculoId);
        for (var i = 0; i < 3; i++) await ResponderAsync(a.EmpresaId, e.VersionId, (e.Unica, e.UnicaSi));

        Assert.Equal(HttpStatusCode.NotFound, (await clienteB.GetAsync(Ruta(e.EncuestaId))).StatusCode);
        Assert.Equal(0, await ContarRiesgosAsync(a.EmpresaId, e.EncuestaId));
    }

    [Fact]
    public async Task Un_colaborador_no_tiene_acceso()
    {
        var empresa = await _escenario.CrearEmpresaAsync("A", 1);
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");
        var e = await CrearEncuestaAsync(empresa.EmpresaId, empresa.AdminVinculoId);

        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.GetAsync(Ruta(e.EncuestaId))).StatusCode);
    }

    // ---------- Apoyo ----------

    private static string Ruta(Guid encuestaId) => $"/api/encuestas/{encuestaId}/peligros-sugeridos";

    private sealed record EncuestaArmada(
        Guid EncuestaId, Guid VersionId, Guid PeligroX, Guid PeligroY, Guid PeligroZ,
        Guid Unica, Guid UnicaSi, Guid UnicaNo, Guid Multiple, Guid MultipleExpone, Guid MultipleNoExpone);

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable)> EmpresaConResponsableAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("A", 1);
        return (empresa, await _escenario.ClienteConRolAsync(empresa, "responsable_sst"));
    }

    private static Dictionary<Guid, JsonElement> Sugerencias(JsonElement r) =>
        r.GetProperty("sugerencias").EnumerateArray().ToDictionary(s => s.GetProperty("peligroId").GetGuid());

    private async Task<int> ContarRiesgosAsync(Guid empresaId, Guid encuestaId)
    {
        await using var db = api.NuevoContexto(empresaId);
        return await db.RiesgoEmpresa.CountAsync(r => r.EncuestaId == encuestaId);
    }

    /// <summary>
    /// Encuesta publicada con: una pregunta de selección única (peligro X; "Sí" indica exposición) y una de
    /// selección múltiple (peligro Y; una opción indica exposición). Crea también el peligro Z para cambios.
    /// </summary>
    private async Task<EncuestaArmada> CrearEncuestaAsync(Guid empresaId, Guid creadorId)
    {
        await using var db = api.NuevoContexto(empresaId);
        var tipos = await db.TipoRespuestaPregunta.ToDictionaryAsync(t => t.Codigo, t => t.Id);
        var x = NuevoPeligro(); var y = NuevoPeligro(); var z = NuevoPeligro();
        db.Peligro.AddRange(x, y, z);

        var encuesta = new Encuesta { Titulo = "Diagnóstico", Estado = EstadoEncuesta.Publicada, CreadaPorId = creadorId };
        var version = new EncuestaVersion
        {
            Encuesta = encuesta, NumeroVersion = 1, Estado = EstadoVersionEncuesta.Publicada,
            CreadaPorId = creadorId, PublicadaEn = DateTime.UtcNow
        };
        var unica = new EncuestaPregunta
        {
            EncuestaVersion = version, Texto = "¿Levantas cargas pesadas?", Orden = 1, PeligroId = x.Id,
            TipoRespuestaPreguntaId = tipos[TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica]
        };
        var si = new EncuestaOpcion { EncuestaPregunta = unica, Texto = "Sí", Orden = 1, IndicaExposicion = true };
        var no = new EncuestaOpcion { EncuestaPregunta = unica, Texto = "No", Orden = 2 };
        var multiple = new EncuestaPregunta
        {
            EncuestaVersion = version, Texto = "¿Qué haces en tu puesto?", Orden = 2, PeligroId = y.Id,
            TipoRespuestaPreguntaId = tipos[TipoRespuestaPreguntaCatalogo.CodigoSeleccionMultiple]
        };
        var expone = new EncuestaOpcion { EncuestaPregunta = multiple, Texto = "Uso químicos", Orden = 1, IndicaExposicion = true };
        var noExpone = new EncuestaOpcion { EncuestaPregunta = multiple, Texto = "Trabajo en escritorio", Orden = 2 };
        db.AddRange(encuesta, version, unica, si, no, multiple, expone, noExpone);
        await db.SaveChangesAsync();

        return new EncuestaArmada(encuesta.Id, version.Id, x.Id, y.Id, z.Id,
            unica.Id, si.Id, no.Id, multiple.Id, expone.Id, noExpone.Id);
    }

    /// <summary>Reemplaza la versión 1 por una versión 2 con la pregunta única apuntando a otro peligro.</summary>
    private async Task<EncuestaArmada> CrearVersionConPeligroAsync(Guid empresaId, Guid creadorId, EncuestaArmada e, Guid nuevoPeligro)
    {
        await using var db = api.NuevoContexto(empresaId);
        var v1 = await db.EncuestaVersion.SingleAsync(v => v.Id == e.VersionId);
        v1.Estado = EstadoVersionEncuesta.Reemplazada;
        await db.SaveChangesAsync();

        var tipoUnica = await db.TipoRespuestaPregunta
            .Where(t => t.Codigo == TipoRespuestaPreguntaCatalogo.CodigoSeleccionUnica).Select(t => t.Id).SingleAsync();
        var v2 = new EncuestaVersion
        {
            EncuestaId = e.EncuestaId, NumeroVersion = 2, Estado = EstadoVersionEncuesta.Publicada,
            CreadaPorId = creadorId, PublicadaEn = DateTime.UtcNow
        };
        var unica = new EncuestaPregunta
        {
            EncuestaVersion = v2, Texto = "¿Levantas cargas pesadas?", Orden = 1, PeligroId = nuevoPeligro,
            TipoRespuestaPreguntaId = tipoUnica
        };
        var si = new EncuestaOpcion { EncuestaPregunta = unica, Texto = "Sí", Orden = 1, IndicaExposicion = true };
        var no = new EncuestaOpcion { EncuestaPregunta = unica, Texto = "No", Orden = 2 };
        db.AddRange(v2, unica, si, no);
        await db.SaveChangesAsync();
        return e with { VersionId = v2.Id, Unica = unica.Id, UnicaSi = si.Id, UnicaNo = no.Id };
    }

    /// <summary>Una respuesta anónima. Si una pregunta aparece varias veces, es de selección múltiple.</summary>
    private async Task ResponderAsync(Guid empresaId, Guid versionId, params (Guid Pregunta, Guid Opcion)[] elegidas)
    {
        await using var db = api.NuevoContexto(empresaId);
        var respuesta = new RespuestaEncuesta { EncuestaVersionId = versionId, FechaRespuesta = DateOnly.FromDateTime(DateTime.UtcNow) };
        db.RespuestaEncuesta.Add(respuesta);
        foreach (var grupo in elegidas.GroupBy(x => x.Pregunta))
        {
            var multiple = grupo.Count() > 1 || await db.EncuestaPregunta
                .Where(p => p.Id == grupo.Key).Select(p => p.TipoRespuestaPregunta.Codigo)
                .SingleAsync() == TipoRespuestaPreguntaCatalogo.CodigoSeleccionMultiple;
            var detalle = new DetalleRespuesta
            {
                RespuestaEncuestaId = respuesta.Id, EncuestaPreguntaId = grupo.Key,
                EncuestaOpcionId = multiple ? null : grupo.Single().Opcion
            };
            db.DetalleRespuesta.Add(detalle);
            if (multiple)
                foreach (var (pregunta, opcion) in grupo)
                    db.DetalleRespuestaOpcion.Add(new DetalleRespuestaOpcion
                    {
                        RespuestaEncuestaId = respuesta.Id, EncuestaPreguntaId = pregunta, EncuestaOpcionId = opcion
                    });
        }
        await db.SaveChangesAsync();
    }

    private static Peligro NuevoPeligro() => new()
    {
        Codigo = $"p{Guid.NewGuid():N}"[..20], Nombre = $"Peligro {Guid.NewGuid():N}"[..20],
        Clasificacion = ClasificacionPeligro.Ergonomico
    };
}
