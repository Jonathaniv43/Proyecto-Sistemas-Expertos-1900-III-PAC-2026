using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Norvia.Domain.Entities.Notificaciones;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Notificaciones;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Notificaciones;

/// <summary>
/// HU-118: al crear la notificación se intenta el correo de inmediato (RN-78); si falla, queda pendiente para
/// el poller de fondo, que la usa como respaldo con reintentos. Las notificaciones salen de asignar tareas (HU-081).
/// El poller está apagado en las pruebas: cada una lo dispara cuando le conviene (<see cref="ProcesarBandejaAsync"/>).
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class CorreoNotificacionesTests(NorviaApiFactory api)
{
    private const int MaxIntentos = 5; // Valor por defecto de Notificaciones__MaxIntentos.
    private static readonly string Futura = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)).ToString("yyyy-MM-dd");

    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Asignar_una_tarea_envia_el_correo_de_inmediato()
    {
        var (empresa, email, colaborador, tarea) = await TareaParaColaboradorAsync("Revisar <extintores> & señalización");

        var enviada = await NotificacionDeAsync(empresa.EmpresaId, tarea);
        Assert.Equal(colaborador, enviada.DestinatarioId);
        Assert.Equal(TipoNotificacion.TareaAsignada, enviada.Tipo);
        Assert.Equal("Te asignaron una tarea", enviada.Titulo);
        Assert.Equal(EstadoCorreo.Enviado, enviada.CorreoEstado);
        Assert.Equal(1, enviada.CorreoIntentos);
        Assert.NotNull(enviada.CorreoEnviadoEn);
        Assert.Null(enviada.CorreoUltimoError);

        var correo = Assert.Single(api.Correos.Para(email).Where(EsDeNotificacion));
        Assert.Equal("Norvia — Te asignaron una tarea", correo.Asunto);
        // El texto de la tarea va codificado para HTML: no puede inyectar marcas en el correo.
        Assert.Contains("Revisar &lt;extintores&gt; &amp; se", correo.CuerpoHtml);
        Assert.DoesNotContain("<extintores>", correo.CuerpoHtml);
        Assert.Contains("/mis-tareas", correo.CuerpoHtml);

        // El poller de fondo no reenvía lo que ya se envió de inmediato.
        await ProcesarBandejaAsync();
        Assert.Single(api.Correos.Para(email).Where(EsDeNotificacion));
    }

    [Fact]
    public async Task Si_el_intento_inmediato_falla_queda_pendiente_y_el_poller_la_reintenta_despues_de_esperar()
    {
        var (empresa, email, colaborador) = await PrepararColaboradorAsync();
        var tarea = await ConSmtpCaidoAsync(() => CrearTareaAsync(empresa, colaborador, "Señalizar salidas"));

        var fallida = await NotificacionDeAsync(empresa.EmpresaId, tarea);
        Assert.Equal(EstadoCorreo.Pendiente, fallida.CorreoEstado);
        Assert.Equal(1, fallida.CorreoIntentos);
        Assert.Equal("SMTP de prueba caído", fallida.CorreoUltimoError);
        Assert.True(fallida.CorreoProximoIntentoEn > DateTime.UtcNow, "El reintento espera un tiempo.");
        Assert.Empty(api.Correos.Para(email).Where(EsDeNotificacion));

        // Antes de que venza la espera no se vuelve a intentar, aunque el SMTP ya funcione.
        await ProcesarBandejaAsync();
        Assert.Equal(1, (await NotificacionDeAsync(empresa.EmpresaId, tarea)).CorreoIntentos);
        Assert.Empty(api.Correos.Para(email).Where(EsDeNotificacion));

        await VencerEsperaAsync(empresa.EmpresaId, fallida.Id);
        await ProcesarBandejaAsync();

        var enviada = await NotificacionDeAsync(empresa.EmpresaId, tarea);
        Assert.Equal(EstadoCorreo.Enviado, enviada.CorreoEstado);
        Assert.Equal(2, enviada.CorreoIntentos);
        Assert.Null(enviada.CorreoUltimoError);
        Assert.Single(api.Correos.Para(email).Where(EsDeNotificacion));
    }

    [Fact]
    public async Task La_espera_entre_intentos_crece_con_cada_fallo()
    {
        var (empresa, _, colaborador) = await PrepararColaboradorAsync();
        var empresaId = empresa.EmpresaId;
        var esperas = new List<TimeSpan>();

        await ConSmtpCaidoAsync(async () =>
        {
            // El primer fallo es el intento inmediato al crear la tarea.
            var tareaId = await CrearTareaAsync(empresa, colaborador, "Inspeccionar botiquines");
            var primera = await NotificacionDeAsync(empresaId, tareaId);
            esperas.Add(primera.CorreoProximoIntentoEn!.Value - DateTime.UtcNow);
            await VencerEsperaAsync(empresaId, primera.Id);

            for (var i = 0; i < 2; i++)
            {
                var antes = DateTime.UtcNow;
                await ProcesarBandejaAsync();
                var n = await NotificacionDeAsync(empresaId, tareaId);
                esperas.Add(n.CorreoProximoIntentoEn!.Value - antes);
                await VencerEsperaAsync(empresaId, n.Id);
            }
        });

        // 1, 2 y 4 minutos (con margen por la hora de la base y la duración de la prueba).
        Assert.InRange(esperas[0].TotalSeconds, 50, 70);
        Assert.InRange(esperas[1].TotalSeconds, 110, 130);
        Assert.InRange(esperas[2].TotalSeconds, 230, 250);
    }

    [Fact]
    public async Task Al_agotar_los_intentos_queda_fallido_sin_revertir_la_tarea_ni_ocultar_la_notificacion()
    {
        var (empresa, email, colaborador) = await PrepararColaboradorAsync();
        var tarea = await ConSmtpCaidoAsync(() => CrearTareaAsync(empresa, colaborador, "Capacitar brigada"));

        await ConSmtpCaidoAsync(async () =>
        {
            // El intento inmediato al crear la tarea ya gastó el primero.
            for (var i = 1; i < MaxIntentos; i++)
            {
                await VencerEsperaAsync(empresa.EmpresaId, (await NotificacionDeAsync(empresa.EmpresaId, tarea)).Id);
                await ProcesarBandejaAsync();
            }
        });

        var fallida = await NotificacionDeAsync(empresa.EmpresaId, tarea);
        Assert.Equal(EstadoCorreo.Fallido, fallida.CorreoEstado);
        Assert.Equal(MaxIntentos, fallida.CorreoIntentos);
        Assert.Equal("SMTP de prueba caído", fallida.CorreoUltimoError);
        Assert.Null(fallida.CorreoProximoIntentoEn);
        Assert.Null(fallida.LeidaEn);

        // Fallido es definitivo: con el SMTP de vuelta ya no se intenta.
        await ProcesarBandejaAsync();
        Assert.Equal(MaxIntentos, (await NotificacionDeAsync(empresa.EmpresaId, tarea)).CorreoIntentos);
        Assert.Empty(api.Correos.Para(email).Where(EsDeNotificacion));

        // La tarea sigue asignada: el correo no revierte la operación que originó la notificación.
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.Equal(colaborador, (await db.Tarea.SingleAsync(t => t.Id == tarea)).ResponsableId);
    }

    [Fact]
    public async Task No_escribe_a_quien_ya_no_tiene_acceso_a_la_empresa()
    {
        // El intento inmediato falla (SMTP caído) para que la fila quede pendiente antes de suspender el vínculo.
        var (empresa, email, colaborador) = await PrepararColaboradorAsync();
        var tarea = await ConSmtpCaidoAsync(() => CrearTareaAsync(empresa, colaborador, "Revisar arneses"));
        var pendiente = await NotificacionDeAsync(empresa.EmpresaId, tarea);

        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            var vinculo = await db.VinculoEmpresa.SingleAsync(v => v.Id == colaborador);
            vinculo.Estado = EstadoVinculo.Suspendido;
            await db.SaveChangesAsync();
        }

        await VencerEsperaAsync(empresa.EmpresaId, pendiente.Id);
        await ProcesarBandejaAsync();

        var n = await NotificacionDeAsync(empresa.EmpresaId, tarea);
        Assert.Equal(EstadoCorreo.Fallido, n.CorreoEstado);
        Assert.Equal(1, n.CorreoIntentos);
        Assert.Empty(api.Correos.Para(email).Where(EsDeNotificacion));
    }

    [Fact]
    public async Task Quien_se_asigna_una_tarea_a_si_mismo_no_recibe_notificacion()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Autoasignada");

        var r = await empresa.Cliente.PostAsJsonAsync("/api/plan-trabajo/tareas",
            new { titulo = "Mi propia tarea", prioridad = 2, responsableId = empresa.AdminVinculoId, fechaLimite = Futura });

        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var tarea = (await Escenario.Json(r)).GetProperty("id").GetGuid();
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.False(await db.Notificacion.AnyAsync(n => n.TareaId == tarea));
    }

    [Fact]
    public async Task Reasignar_avisa_al_nuevo_responsable_y_cambiar_solo_la_fecha_no_vuelve_a_avisar()
    {
        var (empresa, _, primero, tarea) = await TareaParaColaboradorAsync("Medir ruido");
        var segundo = await Escenario.AgregarColaboradorAsync(empresa, $"c.{Guid.NewGuid():N}@pruebas.hn");
        var url = $"/api/plan-trabajo/tareas/{tarea}/asignacion";
        var otraFecha = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(40)).ToString("yyyy-MM-dd");

        Assert.Equal(HttpStatusCode.OK, (await empresa.Cliente.PutAsJsonAsync(url, new { responsableId = segundo, fechaLimite = Futura })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await empresa.Cliente.PutAsJsonAsync(url, new { responsableId = segundo, fechaLimite = otraFecha })).StatusCode);

        await using var db = api.NuevoContexto(empresa.EmpresaId);
        var destinatarios = await db.Notificacion.Where(n => n.TareaId == tarea).Select(n => n.DestinatarioId).ToListAsync();
        Assert.Equal(2, destinatarios.Count);
        Assert.Contains(primero, destinatarios);
        Assert.Contains(segundo, destinatarios);
    }

    /// <summary>Empresa con un colaborador al que su administrador le crea una tarea manual (HU-043), ya asignada.</summary>
    private async Task<(EmpresaDePrueba Empresa, string Email, Guid Colaborador, Guid Tarea)> TareaParaColaboradorAsync(string titulo)
    {
        var (empresa, email, colaborador) = await PrepararColaboradorAsync();
        var tarea = await CrearTareaAsync(empresa, colaborador, titulo);
        return (empresa, email, colaborador, tarea);
    }

    /// <summary>Empresa con un colaborador activo, sin tarea todavía (para controlar el SMTP solo durante la creación).</summary>
    private async Task<(EmpresaDePrueba Empresa, string Email, Guid Colaborador)> PrepararColaboradorAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Notificaciones");
        var email = $"c.{Guid.NewGuid():N}@pruebas.hn";
        var colaborador = await Escenario.AgregarColaboradorAsync(empresa, email);
        return (empresa, email, colaborador);
    }

    private static async Task<Guid> CrearTareaAsync(EmpresaDePrueba empresa, Guid responsable, string titulo)
    {
        var r = await empresa.Cliente.PostAsJsonAsync("/api/plan-trabajo/tareas",
            new { titulo, prioridad = 1, responsableId = responsable, fechaLimite = Futura });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private async Task<Notificacion> NotificacionDeAsync(Guid empresaId, Guid tareaId)
    {
        await using var db = api.NuevoContexto(empresaId);
        return await db.Notificacion.AsNoTracking().SingleAsync(n => n.TareaId == tareaId);
    }

    /// <summary>Lo que hace el poller de fondo en cada vuelta.</summary>
    private async Task ProcesarBandejaAsync()
    {
        using var scope = api.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<EnvioNotificacionesPendientes>().ProcesarAsync(CancellationToken.None);
    }

    /// <summary>Simula que ya pasó la espera hasta el próximo intento.</summary>
    private async Task VencerEsperaAsync(Guid empresaId, Guid notificacionId)
    {
        await using var db = api.NuevoContexto(empresaId);
        var n = await db.Notificacion.SingleAsync(x => x.Id == notificacionId);
        if (n.CorreoProximoIntentoEn is null) return;
        n.CorreoProximoIntentoEn = DateTime.UtcNow.AddHours(-1);
        await db.SaveChangesAsync();
    }

    private async Task<T> ConSmtpCaidoAsync<T>(Func<Task<T>> accion)
    {
        api.Correos.Fallar = true;
        try
        {
            return await accion();
        }
        finally
        {
            api.Correos.Fallar = false;
        }
    }

    private async Task ConSmtpCaidoAsync(Func<Task> accion)
    {
        api.Correos.Fallar = true;
        try
        {
            await accion();
        }
        finally
        {
            api.Correos.Fallar = false;
        }
    }

    private static bool EsDeNotificacion(Application.Common.Correo.MensajeCorreo m) => m.Asunto.StartsWith("Norvia — Te asignaron");
}
