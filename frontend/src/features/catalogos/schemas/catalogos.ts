import { textoRequerido, z } from '@/shared/validacion'

/*
 * Espejo de los validadores de catálogos del backend (HU-023). El código de rubros, peligros y plantillas de documento
 * se fija al crear: minúsculas, números y guion bajo.
 */

const codigo = (maximo: number) =>
  z
    .string()
    .trim()
    .min(1, 'Este campo es obligatorio.')
    .max(maximo, `Debe tener como máximo ${maximo} caracteres.`)
    .regex(/^[a-z0-9_]+$/, 'Usa solo minúsculas, números y guion bajo, sin espacios ni tildes.')

const descripcion = z.string().max(2000, 'Debe tener como máximo 2000 caracteres.')

export const rubroEsquema = z.object({ codigo: codigo(30), nombre: textoRequerido(80) })

export const categoriaEsquema = z.object({
  rubroId: z.string().min(1, 'Elige el rubro.').transform(Number),
  nombre: textoRequerido(100),
  descripcion,
})

export const peligroEsquema = z.object({
  codigo: codigo(20),
  nombre: textoRequerido(150),
  descripcion,
  clasificacion: z.enum(
    ['fisico', 'mecanico', 'electrico', 'quimico', 'biologico', 'ergonomico', 'psicosocial', 'locativo', 'otro'],
    'Elige la clasificación.',
  ),
})

/** `modelo` = sin tipo de control: tarea modelo del plan. */
export const plantillaTareaEsquema = z.object({
  tipo: z.enum(['eliminacion', 'sustitucion', 'ingenieria', 'administrativo', 'epp', 'modelo'], 'Elige qué es.'),
  titulo: textoRequerido(200),
  descripcion,
  prioridad: z.enum(['1', '2', '3'], 'Elige la prioridad.'),
  requiereEvidencia: z.boolean(),
})

export const documentoEsquema = z.object({
  codigo: codigo(30),
  titulo: textoRequerido(200),
  tipoDocumento: z.enum(['politica', 'procedimiento'], 'Elige el tipo.'),
  /** '' = todos los rubros. */
  rubroId: z.string(),
  contenido: textoRequerido(100_000),
})

/**
 * Pregunta exploratoria (D26, RN-68), espejo de ReglasPregunta: respuesta breve sin opciones ni peligro; selección
 * con 2 a 20 opciones sin repetir; con peligro, al menos una opción indica exposición; sin peligro, ninguna.
 */
export const preguntaEsquema = z
  .object({
    categoriaRiesgoId: z.string().min(1, 'Elige la categoría.'),
    texto: textoRequerido(500),
    tipoRespuestaCodigo: z.enum(['seleccion_unica', 'seleccion_multiple', 'respuesta_breve'], 'Elige el tipo de respuesta.'),
    obligatoria: z.boolean(),
    peligroId: z.string(),
    opciones: z.array(
      z.object({
        texto: z.string().trim().max(200, 'Debe tener como máximo 200 caracteres.'),
        indicaExposicion: z.boolean(),
      }),
    ),
  })
  .check((ctx) => {
    const v = ctx.value
    if (v.tipoRespuestaCodigo === 'respuesta_breve') return
    const llenas = v.opciones.filter((o) => o.texto.trim())
    v.opciones.forEach((o, i) => {
      if (!o.texto.trim()) ctx.issues.push({ code: 'custom', input: o.texto, path: ['opciones', i, 'texto'], message: 'Escribe la opción o quítala.' })
    })
    if (llenas.length < 2 || llenas.length > 20)
      ctx.issues.push({ code: 'custom', input: v.opciones, path: ['opciones'], message: 'Una pregunta de selección necesita entre 2 y 20 opciones.' })
    const vistos = new Set<string>()
    v.opciones.forEach((o, i) => {
      const clave = o.texto.trim().toLowerCase()
      if (clave && vistos.has(clave))
        ctx.issues.push({ code: 'custom', input: o.texto, path: ['opciones', i, 'texto'], message: 'Esta opción está repetida.' })
      vistos.add(clave)
    })
    if (v.peligroId && !v.opciones.some((o) => o.indicaExposicion))
      ctx.issues.push({
        code: 'custom',
        input: v.opciones,
        path: ['opciones'],
        message: 'Marca al menos una opción que indique exposición al peligro.',
      })
  })
