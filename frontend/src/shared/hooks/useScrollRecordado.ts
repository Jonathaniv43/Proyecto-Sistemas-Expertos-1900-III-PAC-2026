import { useEffect, useRef } from 'react'
import { useLocation } from 'react-router-dom'

const clave = (ruta: string) => `scroll:${ruta}`

/**
 * Recuerda la posición de desplazamiento de una pantalla padre (lista, detalle) para que, al volver
 * con "Volver" (design-system.md § Volver), quede donde se dejó.
 *
 * `listo`: los datos ya están en pantalla. Las páginas cargan con `usePeticion` (no con loaders), así
 * que hay que esperar al contenido para que la página tenga alto suficiente antes de restaurar.
 * Recién después de restaurar se empieza a guardar, para no pisar la posición con el salto al inicio.
 */
export function useScrollRecordado(listo: boolean) {
  const { pathname } = useLocation()
  const restaurado = useRef(false)

  useEffect(() => {
    if (!listo) return

    if (!restaurado.current) {
      restaurado.current = true
      const guardado = Number(sessionStorage.getItem(clave(pathname)))
      if (guardado > 0) window.scrollTo(0, guardado)
    }

    const guardar = () => {
      // Al navegar, el salto al inicio de la página nueva puede disparar este evento: se ignora.
      if (window.location.pathname !== pathname) return
      sessionStorage.setItem(clave(pathname), String(Math.round(window.scrollY)))
    }
    window.addEventListener('scroll', guardar, { passive: true })
    return () => window.removeEventListener('scroll', guardar)
  }, [listo, pathname])
}
