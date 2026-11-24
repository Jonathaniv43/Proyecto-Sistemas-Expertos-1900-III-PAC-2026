import { textoAIso, z } from '@/shared/validacion'

/** Igual que FiltrosEstadisticasValidator.MesesMaximos (backend). */
export const MESES_MAXIMOS = 24

/** "2026-11-24" + n meses, en aaaa-mm-dd (el día se ajusta al último del mes si no existe). */
export const sumarMeses = (iso: string, meses: number) => {
  const [a, m, d] = iso.split('-').map(Number)
  const ultimo = new Date(Date.UTC(a, m - 1 + meses + 1, 0)).getUTCDate()
  const f = new Date(Date.UTC(a, m - 1 + meses, Math.min(d, ultimo)))
  return f.toISOString().slice(0, 10)
}

const sumarDias = (iso: string, dias: number) => {
  const f = new Date(`${iso}T00:00:00Z`)
  f.setUTCDate(f.getUTCDate() + dias)
  return f.toISOString().slice(0, 10)
}

/**
 * Espejo de FiltrosEstadisticasValidator (HU-052): desde y hasta obligatorios (dd/mm/aaaa), hasta no anterior a desde y
 * como máximo 24 meses. Sale en aaaa-mm-dd.
 */
export const periodoEsquema = z
  .object({
    desde: z.string().trim().min(1, 'Indica desde qué fecha.'),
    hasta: z.string().trim().min(1, 'Indica hasta qué fecha.'),
  })
  .check((ctx) => {
    const v = ctx.value
    const desde = v.desde ? textoAIso(v.desde) : null
    const hasta = v.hasta ? textoAIso(v.hasta) : null
    const formato = 'Escribe la fecha como dd/mm/aaaa, por ejemplo 01/01/2026.'
    if (v.desde && !desde) ctx.issues.push({ code: 'custom', input: v.desde, path: ['desde'], message: formato })
    if (v.hasta && !hasta) ctx.issues.push({ code: 'custom', input: v.hasta, path: ['hasta'], message: formato })
    if (!desde || !hasta) return
    if (hasta < desde)
      ctx.issues.push({ code: 'custom', input: v.hasta, path: ['hasta'], message: 'No puede ser anterior a la fecha inicial.' })
    else if (hasta > sumarDias(sumarMeses(desde, MESES_MAXIMOS), -1))
      ctx.issues.push({ code: 'custom', input: v.hasta, path: ['hasta'], message: `El periodo puede abarcar como máximo ${MESES_MAXIMOS} meses.` })
  })
  .transform((v) => ({ desde: textoAIso(v.desde)!, hasta: textoAIso(v.hasta)! }))
