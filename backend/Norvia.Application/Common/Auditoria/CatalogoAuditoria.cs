namespace Norvia.Application.Common.Auditoria;

/// <summary>Entidades auditables en <see cref="Domain.Entities.Auditoria.RegistroAuditoria"/> (HU-099).</summary>
public static class EntidadesAuditoria
{
    public const string Documento = "Documento";
    public const string VinculoEmpresa = "VinculoEmpresa";
    public const string Encuesta = "Encuesta";
    public const string Accidente = "Accidente";
    public const string RiesgoEmpresa = "RiesgoEmpresa";
    public const string EvaluacionRiesgo = "EvaluacionRiesgo";
    public const string Control = "Control";
    public const string Tarea = "Tarea";
    public const string Empresa = "Empresa";

    public static readonly string[] Todas =
    [
        Documento,
        VinculoEmpresa,
        Encuesta,
        Accidente,
        RiesgoEmpresa,
        EvaluacionRiesgo,
        Control,
        Tarea,
        Empresa
    ];
}

/// <summary>Códigos estándar de acción en <see cref="Domain.Entities.Auditoria.RegistroAuditoria"/> (HU-099).</summary>
public static class AccionesAuditoria
{
    // M1 — Vínculos ("cambios de grupo")
    public const string VinculoCreado = "vinculo_creado";
    public const string VinculoActualizado = "vinculo_actualizado";
    public const string EncuestadoCreado = "encuestado_creado";

    // M2 — Empresa
    public const string EmpresaPerfilActualizado = "empresa_perfil_actualizado";

    // M3 — Encuestas (excluye respuestas anónimas por RN-72)
    public const string EncuestaCreada = "encuesta_creada";
    public const string EncuestaPublicada = "encuesta_publicada";
    public const string EncuestaCerrada = "encuesta_cerrada";

    // M4 — Matriz de riesgos
    public const string PeligroSugeridoRevisado = "peligro_sugerido_revisado";
    public const string RiesgoCreado = "riesgo_creado";
    public const string RiesgoEvaluado = "riesgo_evaluado";
    public const string RiesgoEvaluadoResidual = "riesgo_evaluado_residual";
    public const string ControlCreado = "control_creado";

    // M5 — Plan de trabajo
    public const string PlanGenerado = "plan_generado";
    public const string TareaCreada = "tarea_creada";
    public const string TareaAsignada = "tarea_asignada";
    public const string TareaEstadoCambiado = "tarea_estado_cambiado";

    // M6 — Eventos / accidentes (excluye reportes anónimos por D11)
    public const string EventoInvestigado = "evento_investigado";
    public const string MedidaCorrectivaCreada = "medida_correctiva_creada";
    public const string EventoCerrado = "evento_cerrado";
    public const string EventoDescartado = "evento_descartado";

    // M9 — Documentos
    public const string DocumentoGenerado = "documento_generado";
    public const string DocumentoAprobado = "documento_aprobado";
}
