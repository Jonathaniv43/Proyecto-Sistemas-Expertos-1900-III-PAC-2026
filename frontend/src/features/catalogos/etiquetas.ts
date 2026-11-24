import type { ClasificacionPeligro, TipoControl, TipoDocumento, TipoRespuesta } from './types/catalogos'

export const CLASIFICACIONES: Record<ClasificacionPeligro, string> = {
  fisico: 'Físico',
  mecanico: 'Mecánico',
  electrico: 'Eléctrico',
  quimico: 'Químico',
  biologico: 'Biológico',
  ergonomico: 'Ergonómico',
  psicosocial: 'Psicosocial',
  locativo: 'Locativo',
  otro: 'Otro',
}

/** Jerarquía de controles (RN-21), del más efectivo al menos efectivo. */
export const TIPOS_CONTROL: { valor: TipoControl; texto: string }[] = [
  { valor: 'eliminacion', texto: 'Eliminación' },
  { valor: 'sustitucion', texto: 'Sustitución' },
  { valor: 'ingenieria', texto: 'Ingeniería' },
  { valor: 'administrativo', texto: 'Administrativo' },
  { valor: 'epp', texto: 'Equipo de protección personal' },
]
export const NOMBRE_TIPO_CONTROL = Object.fromEntries(TIPOS_CONTROL.map((t) => [t.valor, t.texto])) as Record<TipoControl, string>

export const TIPOS_RESPUESTA: { valor: TipoRespuesta; texto: string; descripcion: string }[] = [
  { valor: 'seleccion_unica', texto: 'Selección única', descripcion: 'Se elige una opción.' },
  { valor: 'seleccion_multiple', texto: 'Selección múltiple', descripcion: 'Se eligen una o varias opciones.' },
  { valor: 'respuesta_breve', texto: 'Respuesta breve', descripcion: 'Texto libre, sin opciones ni peligro.' },
]
export const NOMBRE_TIPO_RESPUESTA = Object.fromEntries(TIPOS_RESPUESTA.map((t) => [t.valor, t.texto])) as Record<TipoRespuesta, string>

export const TIPOS_DOCUMENTO: Record<TipoDocumento, string> = { politica: 'Política', procedimiento: 'Procedimiento' }

export const PRIORIDADES = { 1: 'Alta', 2: 'Media', 3: 'Baja' } as const

/** Errores del backend de catálogos que van junto a un campo. */
export const ERRORES_CATALOGO = {
  codigoDuplicado: 'catalogos.codigo_duplicado',
  nombreDuplicado: 'catalogos.nombre_duplicado',
} as const
