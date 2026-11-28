import type { Location } from 'react-router-dom'
import { esPublica, pantallaDe } from '@/shared/navegacion'

/**
 * Historial para "Volver" (design-system.md § Volver): recuerda, por cada entrada del historial
 * (`location.key`), la pantalla de la que se llegó. Así "Volver" regresa a la pantalla anterior y la
 * nombra, en lugar de ir siempre a la pantalla padre. Se guarda en sessionStorage para que sobreviva
 * a una recarga (la `key` de cada entrada también sobrevive).
 */

const CLAVE = 'volver:anterior'
const MAXIMO = 200

type Anteriores = Record<string, string>

const ruta = (l: Pick<Location, 'pathname' | 'search'>) => `${l.pathname}${l.search}`

function leer(): Anteriores {
  try {
    return (JSON.parse(sessionStorage.getItem(CLAVE) ?? '{}') as Anteriores) ?? {}
  } catch {
    return {}
  }
}

function guardar(anteriores: Anteriores) {
  const claves = Object.keys(anteriores)
  // Solo las entradas más recientes: el historial de una pestaña no necesita más.
  for (const k of claves.slice(0, Math.max(0, claves.length - MAXIMO))) delete anteriores[k]
  try {
    sessionStorage.setItem(CLAVE, JSON.stringify(anteriores))
  } catch {
    // Sin sessionStorage, "Volver" usa la pantalla padre.
  }
}

interface RouterObservable {
  state: { location: Location }
  subscribe: (fn: (state: { location: Location; historyAction: 'POP' | 'PUSH' | 'REPLACE' }) => void) => unknown
}

/**
 * Registra cada navegación. Se llama al crear el router, antes de montar RouterProvider, para que el
 * registro ocurra antes de que la pantalla nueva se dibuje.
 * - PUSH: la anterior es la pantalla de la que se salió.
 * - REPLACE: hereda la anterior de la entrada reemplazada (atrás vuelve a esa).
 * - POP (atrás/adelante): ya estaba registrada.
 */
export function registrarHistorial(router: RouterObservable) {
  let ultima = router.state.location
  router.subscribe(({ location, historyAction }) => {
    if (location.key === ultima.key) return
    const anteriores = leer()
    if (historyAction === 'PUSH') anteriores[location.key] = ruta(ultima)
    else if (historyAction === 'REPLACE') {
      const heredada = anteriores[ultima.key]
      if (heredada) anteriores[location.key] = heredada
    }
    if (historyAction !== 'POP') guardar(anteriores)
    ultima = location
  })
}

function textoPara(anterior: URL, actual: Location): string | undefined {
  if (anterior.pathname === '/login') {
    // Del paso 2 del login al paso 1; desde otras pantallas, al login.
    return actual.pathname === '/login' && !anterior.searchParams.has('empresa') ? 'Volver a buscar tu empresa' : 'Volver a iniciar sesión'
  }
  // Nombre de la pantalla en la lista única (shared/navegacion/pantallas.ts); sin nombre se usa la pantalla padre.
  return pantallaDe(anterior.pathname)?.volver
}

export interface PantallaAnterior {
  ruta: string
  texto: string
}

/**
 * La pantalla de la que se llegó a la actual, si se puede volver a ella con "atrás". No la hay si se
 * entró por URL, si es la misma pantalla, si es de la otra zona (con/sin sesión) o si no tiene nombre.
 */
export function pantallaAnterior(actual: Location): PantallaAnterior | undefined {
  const guardada = leer()[actual.key]
  if (!guardada || guardada === ruta(actual)) return undefined
  const anterior = new URL(guardada, window.location.origin)
  if (anterior.pathname === actual.pathname && anterior.pathname !== '/login') return undefined
  if (esPublica(anterior.pathname) !== esPublica(actual.pathname)) return undefined
  const texto = textoPara(anterior, actual)
  return texto ? { ruta: guardada, texto } : undefined
}
