import type { ClasificacionPeligro, Riesgo } from './types/riesgos'

export const ETIQUETAS_CLASIFICACION: Record<ClasificacionPeligro, string> = {
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

/** Nombre del peligro del riesgo: el del catálogo o el que describió la empresa. */
export const nombrePeligro = (r: Pick<Riesgo, 'peligroNombre' | 'descripcionPeligro'>) =>
  r.peligroNombre ?? r.descripcionPeligro ?? 'Peligro sin nombre'
