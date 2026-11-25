/**
 * Lector del Markdown de las plantillas y los documentos (D13, 16-arquitectura §9). Solo entiende lo que usan las
 * políticas y los procedimientos: títulos (#, ##, ###), párrafos, listas con viñetas (-, *) o numeradas (1.),
 * **negritas** y *cursivas*. Además reconoce los `[Completar: …]` que deja la generación (RN-49).
 *
 * Devuelve una estructura de datos, no HTML: `ContenidoDocumento` la convierte en elementos de React, así nunca se
 * interpreta HTML escrito en el contenido (un `<script>` se muestra como texto).
 */

export type Linea =
  | { tipo: 'texto'; texto: string }
  | { tipo: 'negrita'; hijos: Linea[] }
  | { tipo: 'cursiva'; hijos: Linea[] }
  | { tipo: 'completar'; texto: string }

export type Bloque =
  | { tipo: 'titulo'; nivel: 1 | 2 | 3; contenido: Linea[] }
  | { tipo: 'parrafo'; contenido: Linea[] }
  | { tipo: 'lista'; ordenada: boolean; inicio: number; items: Linea[][] }

const TITULO = /^(#{1,6})\s+(.*?)\s*#*\s*$/
const VINETA = /^\s*[-*+]\s+(.*)$/
const NUMERO = /^\s*(\d{1,9})[.)]\s+(.*)$/
/** Negrita, `[Completar: …]` o cursiva, en ese orden de prioridad. */
const EN_LINEA = /\*\*(.+?)\*\*|\[Completar:\s*([^\]\n]+?)\s*\]|\*([^*\s](?:[^*]*?[^*\s])?)\*/g

/** Texto con formato de una línea: negritas, cursivas y `[Completar: …]`. */
export function leerEnLinea(texto: string): Linea[] {
  const partes: Linea[] = []
  let desde = 0
  for (const m of texto.matchAll(EN_LINEA)) {
    if (m.index > desde) partes.push({ tipo: 'texto', texto: texto.slice(desde, m.index) })
    if (m[1] !== undefined) partes.push({ tipo: 'negrita', hijos: leerEnLinea(m[1]) })
    else if (m[2] !== undefined) partes.push({ tipo: 'completar', texto: m[2] })
    else partes.push({ tipo: 'cursiva', hijos: leerEnLinea(m[3]) })
    desde = m.index + m[0].length
  }
  if (desde < texto.length) partes.push({ tipo: 'texto', texto: texto.slice(desde) })
  return partes
}

/** Bloques del documento. Las líneas seguidas forman un párrafo; una línea vacía lo cierra. */
export function leerMarkdown(contenido: string): Bloque[] {
  const bloques: Bloque[] = []
  let parrafo: string[] = []
  let lista: { ordenada: boolean; inicio: number; items: string[] } | null = null

  const cerrarParrafo = () => {
    if (parrafo.length) bloques.push({ tipo: 'parrafo', contenido: leerEnLinea(parrafo.join(' ')) })
    parrafo = []
  }
  const cerrarLista = () => {
    if (lista) bloques.push({ tipo: 'lista', ordenada: lista.ordenada, inicio: lista.inicio, items: lista.items.map(leerEnLinea) })
    lista = null
  }

  for (const cruda of contenido.replace(/\r\n?/g, '\n').split('\n')) {
    const linea = cruda.trimEnd()
    if (!linea.trim()) {
      cerrarParrafo()
      cerrarLista()
      continue
    }
    const titulo = TITULO.exec(linea)
    if (titulo) {
      cerrarParrafo()
      cerrarLista()
      const nivel = Math.min(titulo[1].length, 3) as 1 | 2 | 3
      bloques.push({ tipo: 'titulo', nivel, contenido: leerEnLinea(titulo[2]) })
      continue
    }
    const vineta = VINETA.exec(linea)
    const numero = vineta ? null : NUMERO.exec(linea)
    if (vineta || numero) {
      cerrarParrafo()
      const ordenada = numero !== null
      if (lista && lista.ordenada !== ordenada) cerrarLista()
      lista ??= { ordenada, inicio: numero ? Number(numero[1]) : 1, items: [] }
      lista.items.push(vineta ? vineta[1] : numero![2])
      continue
    }
    if (lista) {
      // Una línea pegada a un elemento de la lista lo continúa.
      const items = lista.items
      items[items.length - 1] += ` ${linea.trim()}`
      continue
    }
    parrafo.push(linea.trim())
  }
  cerrarParrafo()
  cerrarLista()
  return bloques
}
