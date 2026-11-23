import type { BorradorFormulario } from '../schemas/borrador'
import { TIPOS_RESPUESTA } from '../schemas/borrador'
import type { GuardarBorradorRequest, SeccionEncuesta, TipoRespuesta, VersionEncuesta } from '../types/encuestas'

const esTipoConocido = (tipo: string): tipo is TipoRespuesta => (TIPOS_RESPUESTA as readonly string[]).includes(tipo)

/** Orden de pantalla: por sección (las preguntas sin sección al final) y, dentro, por su posición. */
export function ordenarPreguntas<P extends { seccionId: string | null; orden: number }>(preguntas: P[], secciones: SeccionEncuesta[]) {
  const ordenSeccion = new Map(secciones.map((s) => [s.id, s.orden]))
  const posicion = (p: P) => (p.seccionId ? (ordenSeccion.get(p.seccionId) ?? Number.MAX_SAFE_INTEGER) : Number.MAX_SAFE_INTEGER)
  return [...preguntas].sort((a, b) => posicion(a) - posicion(b) || a.orden - b.orden)
}

/** Versión del backend → valores del formulario. */
export function desdeVersion(titulo: string, version: VersionEncuesta): BorradorFormulario {
  const secciones = [...version.secciones].sort((a, b) => a.orden - b.orden)
  return {
    titulo,
    instrucciones: version.instrucciones ?? '',
    preguntas: ordenarPreguntas(version.preguntas, secciones).map((p) => ({
      id: p.id,
      seccionId: p.seccionId,
      categoriaRiesgoId: p.categoriaRiesgoId,
      texto: p.texto,
      tipoRespuestaCodigo: esTipoConocido(p.tipoRespuestaCodigo) ? p.tipoRespuestaCodigo : 'respuesta_breve',
      obligatoria: p.obligatoria,
      peligroId: p.peligroId,
      opciones: [...p.opciones].sort((a, b) => a.orden - b.orden).map((o) => ({ id: o.id, texto: o.texto, indicaExposicion: o.indicaExposicion })),
    })),
  }
}

/**
 * Valores validados → cuerpo de PUT /borrador. El orden de preguntas y opciones es el de la pantalla.
 *
 * `seccionesNuevas`: al editar una encuesta publicada sin borrador, el backend crea una versión nueva y
 * usa los ids de sección tal como llegan; reenviar los de la versión publicada chocaría con ellos. Por
 * eso, en ese caso, cada sección viaja con un id nuevo y sus preguntas apuntan a él.
 */
export function aSolicitud(
  datos: { titulo: string; instrucciones?: string; preguntas: BorradorFormulario['preguntas'] },
  secciones: SeccionEncuesta[],
  { seccionesNuevas }: { seccionesNuevas: boolean },
): GuardarBorradorRequest {
  const ids = new Map(secciones.map((s) => [s.id, seccionesNuevas ? crypto.randomUUID() : s.id]))
  return {
    titulo: datos.titulo,
    instrucciones: datos.instrucciones?.trim() ? datos.instrucciones : null,
    secciones: secciones.map((s, i) => ({ id: ids.get(s.id)!, titulo: s.titulo, orden: i + 1 })),
    preguntas: datos.preguntas.map((p, i) => {
      const breve = p.tipoRespuestaCodigo === 'respuesta_breve'
      return {
        id: p.id,
        seccionId: p.seccionId ? (ids.get(p.seccionId) ?? null) : null,
        categoriaRiesgoId: p.categoriaRiesgoId,
        texto: p.texto,
        tipoRespuestaCodigo: p.tipoRespuestaCodigo,
        obligatoria: p.obligatoria,
        peligroId: breve ? null : p.peligroId,
        orden: i + 1,
        opciones: breve
          ? []
          : p.opciones.map((o, j) => ({ id: o.id, texto: o.texto.trim(), orden: j + 1, indicaExposicion: p.peligroId ? o.indicaExposicion : false })),
      }
    }),
  }
}
