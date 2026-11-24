import { z } from './zod'

/**
 * Fechas sin hora (design-system.md § Campo de fecha): en pantalla dd/mm/aaaa, en la API aaaa-mm-dd.
 * "Hoy" es el de Honduras (America/Tegucigalpa), igual que el backend, sin importar la zona del navegador.
 */

const dos = (n: number) => String(n).padStart(2, '0')

/** Fecha de hoy en Honduras, en formato de la API (aaaa-mm-dd). */
export const hoyHonduras = () =>
  new Intl.DateTimeFormat('en-CA', { timeZone: 'America/Tegucigalpa', year: 'numeric', month: '2-digit', day: '2-digit' }).format(new Date())

/** "2026-12-15" → "15/12/2026". */
export const isoATexto = (iso: string) => {
  const [a, m, d] = iso.split('-')
  return `${d}/${m}/${a}`
}

/** "15/12/2026" → "2026-12-15", o null si no está completa o la fecha no existe (31/02). */
export const textoAIso = (texto: string): string | null => {
  const partes = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(texto.trim())
  if (!partes) return null
  const [d, m, a] = [Number(partes[1]), Number(partes[2]), Number(partes[3])]
  const fecha = new Date(Date.UTC(a, m - 1, d))
  if (fecha.getUTCFullYear() !== a || fecha.getUTCMonth() !== m - 1 || fecha.getUTCDate() !== d) return null
  return `${a}-${dos(m)}-${dos(d)}`
}

/** Lo que se escribe, con las barras puestas solas: "1512" → "15/12", "15122026" → "15/12/2026". */
export const conBarras = (texto: string) => {
  const digitos = texto.replace(/\D/g, '').slice(0, 8)
  return [digitos.slice(0, 2), digitos.slice(2, 4), digitos.slice(4)].filter(Boolean).join('/')
}

/** "2026-12-15" → "15 dic 2026" (tablas y mensajes). */
export const fechaCorta = (iso: string) =>
  new Date(`${iso}T12:00:00`).toLocaleDateString('es-HN', { day: 'numeric', month: 'short', year: 'numeric' })

/**
 * Fecha límite de una tarea (espejo de AsignarTareaValidator y CrearTareaManualValidator + la regla del servicio):
 * obligatoria y no anterior a hoy. Al editar se conserva la que tenía (`actual`), aunque esté vencida. Sale en aaaa-mm-dd.
 */
export const fechaLimite = (actual?: string | null) =>
  z
    .string()
    .trim()
    .min(1, 'Indica la fecha límite.')
    .transform((texto, ctx) => {
      const iso = textoAIso(texto)
      if (!iso) {
        ctx.addIssue({ code: 'custom', message: 'Escribe la fecha como dd/mm/aaaa, por ejemplo 15/12/2026.' })
        return z.NEVER
      }
      if (iso !== actual && iso < hoyHonduras()) {
        ctx.addIssue({ code: 'custom', message: 'La fecha límite no puede ser anterior a hoy.' })
        return z.NEVER
      }
      return iso
    })
