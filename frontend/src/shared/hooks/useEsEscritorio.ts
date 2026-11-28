import { useSyncExternalStore } from 'react'

/** Desde aquí el menú de la sesión es lateral; debajo, barra superior y panel (mismo corte que `lg` de Tailwind). */
const CONSULTA = '(min-width: 1024px)'

const suscribir = (avisar: () => void) => {
  const consulta = window.matchMedia(CONSULTA)
  consulta.addEventListener('change', avisar)
  return () => consulta.removeEventListener('change', avisar)
}

/** true en escritorio (≥1024px). Cambia al girar el teléfono o al cambiar el ancho de la ventana. */
export const useEsEscritorio = () => useSyncExternalStore(suscribir, () => window.matchMedia(CONSULTA).matches)
