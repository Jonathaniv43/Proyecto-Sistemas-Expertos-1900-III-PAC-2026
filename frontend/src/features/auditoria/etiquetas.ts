import { estadoVolver } from '@/shared/components/ui/estadoVolver'
import type { RegistroAuditoria } from './types/auditoria'

export const ENTIDADES_OPCIONES: readonly { valor: string; texto: string }[] = [
  { valor: 'Documento', texto: 'Documentos' },
  { valor: 'VinculoEmpresa', texto: 'Personas y accesos' },
  { valor: 'Encuesta', texto: 'Encuestas' },
  { valor: 'Accidente', texto: 'Eventos SST' },
  { valor: 'RiesgoEmpresa', texto: 'Riesgos' },
  { valor: 'EvaluacionRiesgo', texto: 'Evaluaciones de riesgo' },
  { valor: 'Control', texto: 'Controles' },
  { valor: 'Tarea', texto: 'Plan de trabajo' },
  { valor: 'Empresa', texto: 'Empresa' },
]

export const ETIQUETAS_ENTIDAD: Record<string, string> = Object.fromEntries(
  ENTIDADES_OPCIONES.map((e) => [e.valor, e.texto]),
)

type InfoAccion = {
  texto: string
  tono: 'neutro' | 'info' | 'exito' | 'advertencia' | 'error'
}

export const INFO_ACCIONES: Record<string, InfoAccion> = {
  vinculo_creado: { texto: 'Persona vinculada', tono: 'info' },
  vinculo_actualizado: { texto: 'Acceso o rol actualizado', tono: 'advertencia' },
  encuestado_creado: { texto: 'Encuestado agregado', tono: 'info' },
  empresa_perfil_actualizado: { texto: 'Perfil de empresa actualizado', tono: 'info' },
  encuesta_creada: { texto: 'Encuesta creada', tono: 'info' },
  encuesta_publicada: { texto: 'Encuesta publicada', tono: 'exito' },
  encuesta_cerrada: { texto: 'Encuesta cerrada', tono: 'neutro' },
  peligro_sugerido_revisado: { texto: 'Peligro sugerido revisado', tono: 'info' },
  riesgo_creado: { texto: 'Riesgo agregado', tono: 'info' },
  riesgo_evaluado: { texto: 'Riesgo evaluado', tono: 'info' },
  riesgo_evaluado_residual: { texto: 'Evaluación residual registrada', tono: 'info' },
  control_creado: { texto: 'Control agregado', tono: 'info' },
  plan_generado: { texto: 'Plan de trabajo generado', tono: 'exito' },
  tarea_creada: { texto: 'Tarea creada', tono: 'info' },
  tarea_asignada: { texto: 'Tarea asignada', tono: 'info' },
  tarea_estado_cambiado: { texto: 'Estado de tarea actualizado', tono: 'info' },
  evento_investigado: { texto: 'Evento investigado', tono: 'info' },
  medida_correctiva_creada: { texto: 'Medida correctiva agregada', tono: 'info' },
  evento_cerrado: { texto: 'Evento cerrado', tono: 'exito' },
  evento_descartado: { texto: 'Evento descartado', tono: 'neutro' },
  documento_generado: { texto: 'Documento generado', tono: 'info' },
  documento_aprobado: { texto: 'Documento aprobado', tono: 'exito' },
}

export function etiquetaAccion(accion: string): InfoAccion {
  return INFO_ACCIONES[accion] ?? { texto: accion.replaceAll('_', ' '), tono: 'neutro' }
}

export function etiquetaEntidad(entidad: string): string {
  return ETIQUETAS_ENTIDAD[entidad] ?? entidad
}

const ETIQUETAS_CLAVE_DETALLE: Record<string, string> = {
  titulo: 'Título',
  persona: 'Persona',
  email: 'Correo',
  rol: 'Rol',
  rolAnterior: 'Rol anterior',
  rolNuevo: 'Rol nuevo',
  estado: 'Estado',
  estadoAnterior: 'Estado anterior',
  estadoNuevo: 'Estado nuevo',
  motivoCodigo: 'Motivo',
  cuentaNueva: 'Cuenta nueva',
  lote: 'Carga en lote',
  razonSocial: 'Razón social',
  rubro: 'Rubro',
  trabajadoresPermanentes: 'Trabajadores permanentes',
  numeroVersion: 'Versión',
  tipoDocumento: 'Tipo',
  categorias: 'Categorías',
  peligro: 'Peligro',
  actividad: 'Actividad',
  tipo: 'Tipo',
  probabilidad: 'Probabilidad',
  severidad: 'Severidad',
  nivel: 'Nivel',
  clasificacion: 'Clasificación',
  descripcion: 'Descripción',
  prioridad: 'Prioridad',
  fechaLimite: 'Fecha límite',
  fechaInvestigacion: 'Fecha de investigación',
  motivo: 'Motivo',
  tareasCreadas: 'Tareas creadas',
  esGrave: 'Grave',
}

function formatearValorDetalle(clave: string, valor: unknown): string | null {
  if (valor === null || valor === undefined || valor === '') return null
  if (typeof valor === 'boolean') return valor ? 'Sí' : 'No'
  if (Array.isArray(valor)) {
    return valor.length > 0 ? valor.map((v) => String(v)).join(', ') : null
  }
  if (typeof valor === 'object') return null
  if (clave === 'motivoCodigo' || clave === 'rol' || clave === 'rolAnterior' || clave === 'rolNuevo') {
    return String(valor).replaceAll('_', ' ')
  }
  return String(valor)
}

/** Convierte el objeto JSON de detalle en fragmentos legibles para humanos, omitiendo IDs técnicos redundantes. */
export function resumirDetalle(detalle: Record<string, unknown> | null): string[] {
  if (!detalle) return []
  const partes: string[] = []
  for (const [clave, valor] of Object.entries(detalle)) {
    if (clave.endsWith('Id') || clave === 'rubroId') continue
    const texto = formatearValorDetalle(clave, valor)
    if (!texto) continue
    const etiqueta = ETIQUETAS_CLAVE_DETALLE[clave] ?? clave
    partes.push(`${etiqueta}: ${texto}`)
  }
  return partes
}

export function destinoDeRegistro(r: RegistroAuditoria): { to: string; texto: string; state?: ReturnType<typeof estadoVolver> } | null {
  const volver = estadoVolver('/auditoria', 'Volver a auditoría')
  switch (r.entidad) {
    case 'Documento':
      return { to: `/documentos/${r.entidadId}`, texto: 'Ver documento', state: volver }
    case 'Encuesta':
      return { to: `/encuestas/${r.entidadId}`, texto: 'Ver encuesta', state: volver }
    case 'Accidente':
      return { to: `/eventos/${r.entidadId}`, texto: 'Ver evento', state: volver }
    case 'RiesgoEmpresa':
      return { to: `/riesgos/${r.entidadId}`, texto: 'Ver riesgo', state: volver }
    case 'EvaluacionRiesgo':
    case 'Control': {
      const riesgoId = typeof r.detalle?.riesgoId === 'string' ? r.detalle.riesgoId : null
      return riesgoId
        ? { to: `/riesgos/${riesgoId}`, texto: 'Ver riesgo', state: volver }
        : { to: '/riesgos', texto: 'Ver matriz', state: volver }
    }
    case 'Tarea':
      return { to: '/plan-de-trabajo', texto: 'Ver plan de trabajo', state: volver }
    case 'VinculoEmpresa':
      return { to: '/personas', texto: 'Ver personas', state: volver }
    case 'Empresa':
      return { to: '/empresa', texto: 'Ver perfil', state: volver }
    default:
      return null
  }
}
