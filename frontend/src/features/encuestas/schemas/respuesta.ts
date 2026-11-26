import { z } from '@/shared/validacion'
import type { EncuestaPendiente, ResponderEncuestaRequest } from '../types/encuestas'

type Pregunta = EncuestaPendiente['preguntas'][number]

/** Valor de cada pregunta en el formulario: texto (breve), id de opción (única) o ids (múltiple). */
export type ValorRespuesta = string | string[]
export type RespuestasFormulario = { respuestas: Record<string, ValorRespuesta> }

/** Ruta del error "responde al menos una": aparte de `respuestas`, para no tapar los errores de cada pregunta. */
export const RUTA_SIN_RESPUESTAS = 'sinRespuestas'

/** Clave de cada pregunta en el formulario (los ids traen guiones; el prefijo evita rutas raras). */
export const claveDe = (preguntaId: string) => `p${preguntaId.replace(/-/g, '')}`

const estaRespondida = (valor: ValorRespuesta | undefined) =>
  Array.isArray(valor) ? valor.length > 0 : Boolean(valor?.trim())

/**
 * Esquema Zod armado a partir del tipo de cada pregunta (16-arquitectura §9), espejo de las reglas de
 * ResponderEncuestaServicio: obligatorias respondidas, texto hasta su máximo, una opción en las de
 * selección única y una o más en las múltiples; y al menos una respuesta en total.
 */
export function esquemaRespuesta(preguntas: Pregunta[]) {
  const forma: Record<string, z.ZodType<ValorRespuesta>> = {}
  for (const p of preguntas) {
    const clave = claveDe(p.id)
    if (p.tipoRespuesta === 'seleccion_multiple') {
      forma[clave] = p.obligatoria ? z.array(z.string()).min(1, 'Elige al menos una opción.') : z.array(z.string())
    } else if (p.tipoRespuesta === 'seleccion_unica') {
      forma[clave] = p.obligatoria ? z.string().min(1, 'Elige una opción.') : z.string()
    } else {
      let texto = z.string()
      if (p.maxCaracteres) texto = texto.refine((t) => t.trim().length <= p.maxCaracteres!, `Escribe como máximo ${p.maxCaracteres} caracteres.`)
      forma[clave] = p.obligatoria ? texto.refine((t) => t.trim().length > 0, 'Esta pregunta es obligatoria.') : texto
    }
  }
  const esquema = z.object({ respuestas: z.object(forma) }).check((ctx) => {
    // Solo si no hay otro error: con preguntas obligatorias vacías ya se marca cada una.
    if (ctx.issues.length === 0 && !Object.values(ctx.value.respuestas).some(estaRespondida)) {
      ctx.issues.push({ code: 'custom', input: ctx.value, path: [RUTA_SIN_RESPUESTAS], message: 'Responde al menos una pregunta antes de enviar.' })
    }
  })
  // Las claves dependen de la encuesta: se tipa como el formulario genérico.
  return esquema as unknown as z.ZodType<RespuestasFormulario, RespuestasFormulario>
}

export const valoresIniciales = (preguntas: Pregunta[]): RespuestasFormulario => ({
  respuestas: Object.fromEntries(preguntas.map((p) => [claveDe(p.id), p.tipoRespuesta === 'seleccion_multiple' ? [] : ''])),
})

/** Formulario → cuerpo del POST. Las preguntas sin responder (opcionales) no se envían. */
export function aSolicitud(encuesta: EncuestaPendiente, valores: RespuestasFormulario): ResponderEncuestaRequest {
  const respuestas: ResponderEncuestaRequest['respuestas'] = []
  for (const p of encuesta.preguntas) {
    const valor = valores.respuestas[claveDe(p.id)]
    if (!estaRespondida(valor)) continue
    if (p.tipoRespuesta === 'seleccion_multiple') respuestas.push({ preguntaId: p.id, opcionIds: valor as string[] })
    else if (p.tipoRespuesta === 'seleccion_unica') respuestas.push({ preguntaId: p.id, opcionId: valor as string })
    else respuestas.push({ preguntaId: p.id, textoLibre: (valor as string).trim() })
  }
  return { versionId: encuesta.versionId, respuestas }
}
