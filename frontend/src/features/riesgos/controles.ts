import type { Riesgo, TipoControl } from './types/riesgos'

/**
 * Jerarquía de controles (RN-21), del más efectivo al menos efectivo. La explicación ayuda a elegir el tipo
 * a quien no conoce la jerarquía.
 */
export const TIPOS_CONTROL: readonly { valor: TipoControl; nombre: string; explicacion: string }[] = [
  { valor: 'eliminacion', nombre: 'Eliminación', explicacion: 'Quitar el peligro del lugar o de la tarea.' },
  { valor: 'sustitucion', nombre: 'Sustitución', explicacion: 'Cambiarlo por algo menos peligroso.' },
  { valor: 'ingenieria', nombre: 'Ingeniería', explicacion: 'Separar a las personas del peligro: guardas, barandas, ventilación.' },
  {
    valor: 'administrativo',
    nombre: 'Administrativo',
    explicacion: 'Cambiar la forma de trabajar: procedimientos, capacitación, señalización, turnos.',
  },
  { valor: 'epp', nombre: 'Equipo de protección personal', explicacion: 'Casco, guantes, lentes, mascarilla. Es la última opción.' },
]

export const NOMBRE_TIPO_CONTROL = Object.fromEntries(TIPOS_CONTROL.map((t) => [t.valor, t.nombre])) as Record<TipoControl, string>

const ORDEN_TIPO = Object.fromEntries(TIPOS_CONTROL.map((t, i) => [t.valor, i])) as Record<TipoControl, number>

/** Del más efectivo al menos efectivo y, dentro de cada tipo, por antigüedad (como los ordena el backend). */
export const ordenarControles = <T extends { tipo: TipoControl; creadoEn: string }>(controles: T[]) =>
  [...controles].sort((a, b) => ORDEN_TIPO[a.tipo] - ORDEN_TIPO[b.tipo] || a.creadoEn.localeCompare(b.creadoEn))

/** RN-19: un riesgo alto o crítico necesita al menos un control. */
export const exigeControl = (r: Pick<Riesgo, 'evaluacion'>) =>
  r.evaluacion?.clasificacion === 'alto' || r.evaluacion?.clasificacion === 'critico'

/** Se señala: alto o crítico y todavía sin controles. */
export const faltaControl = (r: Pick<Riesgo, 'evaluacion' | 'totalControles' | 'estado'>) =>
  r.estado === 'confirmado' && exigeControl(r) && r.totalControles === 0
