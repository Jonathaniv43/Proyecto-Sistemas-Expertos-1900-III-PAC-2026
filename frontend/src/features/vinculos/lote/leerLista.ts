import { filaEncuestadoEsquema } from '../schemas/encuestados'
import type { FilaEncuestado } from '../types/vinculos'

/** Una línea de la lista pegada, ya interpretada y validada. */
export interface LineaLista {
  /** Número de línea en el texto pegado, desde 1 (para que la persona la encuentre). */
  linea: number
  /** Texto original de la línea, para devolverla al cuadro si hay que corregirla. */
  texto: string
  fila?: FilaEncuestado
  error?: string
}

/** Excel pega con tabuladores; un CSV usa punto y coma o coma. */
function separador(linea: string) {
  if (linea.includes('\t')) return '\t'
  if (linea.includes(';')) return ';'
  return ','
}

const pareceEncabezado = (celdas: string[]) => !celdas[0].includes('@') && /correo|email|e-mail/i.test(celdas[0])

/**
 * Lee la lista de encuestados: una persona por línea, con 3 columnas (correo, primer nombre,
 * primer apellido) o 5 (correo, primer nombre, segundo nombre, primer apellido, segundo apellido).
 * Omite líneas vacías y un encabezado en la primera línea. Valida cada fila con las reglas del
 * backend y marca los correos repetidos dentro de la lista.
 */
export function leerLista(texto: string): LineaLista[] {
  const resultado: LineaLista[] = []
  const vistos = new Map<string, number>()

  texto.split(/\r?\n/).forEach((original, indice) => {
    if (!original.trim()) return
    const celdas = original.split(separador(original)).map((c) => c.trim().replace(/^"(.*)"$/, '$1'))
    if (resultado.length === 0 && pareceEncabezado(celdas)) return

    const linea = indice + 1
    const columnas = celdas.length
    if (columnas !== 3 && columnas !== 5) {
      resultado.push({
        linea,
        texto: original,
        error: `Tiene ${columnas} ${columnas === 1 ? 'columna' : 'columnas'}; deben ser 3 o 5.`,
      })
      return
    }

    const [email, primerNombre, a, b, c] = celdas
    const datos =
      columnas === 3
        ? { email, primerNombre, primerApellido: a, segundoNombre: '', segundoApellido: '' }
        : { email, primerNombre, segundoNombre: a, primerApellido: b, segundoApellido: c }

    const validada = filaEncuestadoEsquema.safeParse(datos)
    if (!validada.success) {
      resultado.push({ linea, texto: original, error: validada.error.issues[0].message })
      return
    }

    const clave = validada.data.email.toLowerCase()
    const anterior = vistos.get(clave)
    if (anterior !== undefined) {
      resultado.push({ linea, texto: original, error: `El correo ya está en la línea ${anterior}.` })
      return
    }
    vistos.set(clave, linea)
    resultado.push({ linea, texto: original, fila: validada.data })
  })

  return resultado
}
