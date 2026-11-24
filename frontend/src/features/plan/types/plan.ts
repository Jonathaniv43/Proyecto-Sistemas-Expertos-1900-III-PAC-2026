import type { ClasificacionRiesgo } from '@/shared/riesgo/niveles'

/** Jerarquía de controles (RN-21). */
export type TipoControl = 'eliminacion' | 'sustitucion' | 'ingenieria' | 'administrativo' | 'epp'

/** Tarea que el plan propone y todavía no existe (HU-042). `clave` la identifica al confirmar. */
export interface TareaPropuesta {
  clave: string
  riesgoId: string
  riesgoNombre: string
  riesgoNivel: number
  riesgoClasificacion: ClasificacionRiesgo
  /** `control`: implementa un control del riesgo; `plantilla`: tarea modelo del peligro. */
  origen: 'control' | 'plantilla'
  controlId: string | null
  tipoControl: TipoControl | null
  plantillaId: string | null
  titulo: string
  descripcion: string | null
  /** 1 alta, 2 media, 3 baja: sale del nivel del riesgo. */
  prioridad: 1 | 2 | 3
  requiereEvidencia: boolean
}

/** GET /api/plan-trabajo/propuesta: lo que se agregaría, en orden; no guarda nada. */
export interface PropuestaPlan {
  tareas: TareaPropuesta[]
  /** Riesgos de la matriz que no entran porque no tienen evaluación. */
  riesgosSinEvaluar: number
}

export type EstadoTarea = 'pendiente' | 'en_progreso' | 'completada' | 'cancelada'

/** GET /api/plan-trabajo/tareas. Responsable y fecha límite se asignan en HU-043. */
export interface Tarea {
  id: string
  titulo: string
  descripcion: string | null
  estado: EstadoTarea
  prioridad: 1 | 2 | 3
  responsableId: string | null
  responsableNombre: string | null
  fechaLimite: string | null
  requiereEvidencia: boolean
  origen: 'riesgo' | 'accidente' | 'manual'
  riesgoId: string | null
  riesgoNombre: string | null
  riesgoNivel: number | null
  riesgoClasificacion: ClasificacionRiesgo | null
  controlTipo: TipoControl | null
  creadaEn: string
  completadaEn: string | null
}

/** GET /api/plan-trabajo/responsables: vínculos activos de la empresa que pueden tener tareas (sin encuestados). */
export interface Responsable {
  id: string
  nombreCompleto: string
  rolCodigo: string
}

/** POST /api/plan-trabajo/tareas: tarea manual, ya asignada. */
export interface CrearTareaManualRequest {
  titulo: string
  descripcion?: string
  prioridad: 1 | 2 | 3
  responsableId: string
  /** aaaa-mm-dd */
  fechaLimite: string
  requiereEvidencia: boolean
}
