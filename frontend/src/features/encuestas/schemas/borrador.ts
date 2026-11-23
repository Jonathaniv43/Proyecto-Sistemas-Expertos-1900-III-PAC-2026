import { textoOpcional, textoRequerido, z } from '@/shared/validacion'

/** ActualizarEncuestaBorradorValidator.MaxOpciones */
export const MAXIMO_OPCIONES = 20
export const TIPOS_RESPUESTA = ['respuesta_breve', 'seleccion_unica', 'seleccion_multiple'] as const

const opcion = z.object({
  id: z.string().nullable(),
  texto: z.string(),
  indicaExposicion: z.boolean(),
})

/**
 * Una pregunta del borrador. Espejo de las reglas por pregunta del backend: texto (máx. 500), tipo
 * conocido; las de selección llevan entre 2 y 20 opciones con texto (máx. 200) sin repetir; si la
 * pregunta tiene un peligro, al menos una opción indica exposición.
 */
const pregunta = z
  .object({
    id: z.string().nullable(),
    seccionId: z.string().nullable(),
    categoriaRiesgoId: z.string().nullable(),
    texto: textoRequerido(500),
    tipoRespuestaCodigo: z.enum(TIPOS_RESPUESTA, 'Elige el tipo de respuesta.'),
    obligatoria: z.boolean(),
    peligroId: z.string().nullable(),
    opciones: z.array(opcion),
  })
  .check((ctx) => {
    const { tipoRespuestaCodigo, opciones, peligroId } = ctx.value
    if (tipoRespuestaCodigo === 'respuesta_breve') return
    const problema = (path: (string | number)[], message: string) =>
      ctx.issues.push({ code: 'custom', input: opciones, path, message })

    if (opciones.length < 2 || opciones.length > MAXIMO_OPCIONES) {
      problema(['opciones'], `Una pregunta de selección necesita entre 2 y ${MAXIMO_OPCIONES} opciones.`)
    }
    const vistos = new Map<string, number>()
    opciones.forEach((o, i) => {
      const texto = o.texto.trim()
      if (!texto) return problema(['opciones', i, 'texto'], 'Escribe el texto de la opción.')
      if (texto.length > 200) return problema(['opciones', i, 'texto'], 'Debe tener como máximo 200 caracteres.')
      const clave = texto.toLowerCase()
      if (vistos.has(clave)) problema(['opciones', i, 'texto'], `Repite la opción ${vistos.get(clave)! + 1}.`)
      else vistos.set(clave, i)
    })
    if (peligroId && !opciones.some((o) => o.indicaExposicion)) {
      problema(['opciones'], 'Marca al menos una opción que indique exposición al peligro.')
    }
  })

export const borradorEsquema = z.object({
  titulo: textoRequerido(200),
  instrucciones: textoOpcional(2000),
  preguntas: z.array(pregunta).min(1, 'La encuesta debe tener al menos una pregunta.'),
})

export type BorradorFormulario = z.input<typeof borradorEsquema>
export type BorradorValidado = z.output<typeof borradorEsquema>
export type PreguntaFormulario = BorradorFormulario['preguntas'][number]
