import type { EstadoEnfermedad, EstadoEvento, TipoEvento } from './types/eventos'

export const TIPOS_EVENTO: { valor: TipoEvento; texto: string; descripcion: string }[] = [
  { valor: 'accidente', texto: 'Accidente', descripcion: 'Alguien se lesionó o se enfermó por el trabajo.' },
  { valor: 'incidente', texto: 'Incidente', descripcion: 'Pudo haber lesionado a alguien, pero no pasó.' },
  { valor: 'enfermedad_profesional', texto: 'Enfermedad profesional', descripcion: 'Una enfermedad causada por el trabajo.' },
  { valor: 'condicion_insegura', texto: 'Condición insegura', descripcion: 'Algo que puede causar un accidente, como un cable pelado.' },
]
export const NOMBRE_TIPO = Object.fromEntries(TIPOS_EVENTO.map((t) => [t.valor, t.texto])) as Record<TipoEvento, string>

export const ESTADOS_ENFERMEDAD: { valor: EstadoEnfermedad; texto: string; descripcion: string }[] = [
  { valor: 'sospechosa', texto: 'Sospechosa', descripcion: 'Se cree que la causa el trabajo, pero no lo ha confirmado un médico.' },
  { valor: 'diagnosticada', texto: 'Diagnosticada', descripcion: 'Un médico la confirmó.' },
]

/** Badge de cada estado (design-system.md § Badges de estado: Accidente). */
export const ESTADOS_EVENTO: Record<EstadoEvento, { texto: string; tipo: 'neutro' | 'info' | 'advertencia' }> = {
  abierto: { texto: 'Abierto', tipo: 'advertencia' },
  en_investigacion: { texto: 'En investigación', tipo: 'info' },
  cerrado: { texto: 'Cerrado', tipo: 'neutro' },
  descartado: { texto: 'Descartado', tipo: 'neutro' },
}

/** Fecha y hora en Honduras, p. ej. "24 nov 2026, 2:30 p. m." */
export const fechaHora = (iso: string) =>
  new Date(iso).toLocaleString('es-HN', { timeZone: 'America/Tegucigalpa', dateStyle: 'medium', timeStyle: 'short' })
