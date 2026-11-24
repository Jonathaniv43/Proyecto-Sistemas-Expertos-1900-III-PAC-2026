import { hoyHonduras, leerHora, minutosAhoraHonduras, textoAIso, textoRequerido, z } from '@/shared/validacion'

/**
 * Espejo de ReportarEventoValidator (backend) y de su regla de fecha futura (RN-26): tipo, fecha y hora (no futuras,
 * con un margen de 5 minutos), lugar opcional de hasta 200, descripción obligatoria y, si es enfermedad profesional,
 * si es sospechosa o diagnosticada. Sale con la fecha y hora en ISO con la zona de Honduras.
 */
export const reporteEsquema = z
  .object({
    tipo: z.enum(['accidente', 'incidente', 'enfermedad_profesional', 'condicion_insegura'], 'Elige qué pasó.'),
    estadoEnfermedad: z.string(),
    fecha: z.string().trim().min(1, 'Indica la fecha.'),
    hora: z.string().trim().min(1, 'Indica la hora aproximada.'),
    lugar: z.string().trim().max(200, 'Debe tener como máximo 200 caracteres.'),
    descripcion: textoRequerido(),
    esGrave: z.boolean(),
  })
  .check((ctx) => {
    const v = ctx.value
    if (v.tipo === 'enfermedad_profesional' && !v.estadoEnfermedad)
      ctx.issues.push({ code: 'custom', input: v.estadoEnfermedad, path: ['estadoEnfermedad'], message: 'Indica si es sospechosa o diagnosticada.' })
    const iso = v.fecha ? textoAIso(v.fecha) : null
    if (v.fecha && !iso)
      ctx.issues.push({ code: 'custom', input: v.fecha, path: ['fecha'], message: 'Escribe la fecha como dd/mm/aaaa, por ejemplo 15/11/2026.' })
    else if (iso && iso > hoyHonduras())
      ctx.issues.push({ code: 'custom', input: v.fecha, path: ['fecha'], message: 'La fecha no puede ser futura.' })
    const hora = v.hora ? leerHora(v.hora) : null
    if (v.hora && !hora)
      ctx.issues.push({ code: 'custom', input: v.hora, path: ['hora'], message: 'Escribe la hora como 02:30 p. m.' })
    else if (hora && iso === hoyHonduras() && hora.h * 60 + hora.m > minutosAhoraHonduras() + 5)
      ctx.issues.push({ code: 'custom', input: v.hora, path: ['hora'], message: 'Esa hora todavía no llega.' })
  })
  .transform((v) => {
    const { h, m } = leerHora(v.hora)!
    return {
      tipo: v.tipo,
      estadoEnfermedad: v.tipo === 'enfermedad_profesional' ? (v.estadoEnfermedad as 'sospechosa' | 'diagnosticada') : undefined,
      fechaHoraEvento: `${textoAIso(v.fecha)}T${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:00-06:00`,
      lugar: v.lugar || undefined,
      descripcion: v.descripcion,
      esGrave: v.esGrave,
    }
  })
