import { useEffect, useState } from 'react'

/** Demora antes de mostrar un skeleton (design-system.md § Estados de pantalla). */
const DEMORA_MS = 300

/**
 * Devuelve true solo si `cargando` sigue activo después de ~300ms, para que las cargas rápidas
 * no hagan parpadear un skeleton. Los spinners de botón usan `cargando` directo, sin demora.
 */
export function useCargaVisible(cargando: boolean, demoraMs = DEMORA_MS): boolean {
  const [visible, setVisible] = useState(false)

  useEffect(() => {
    if (!cargando) return
    const temporizador = setTimeout(() => setVisible(true), demoraMs)
    return () => {
      clearTimeout(temporizador)
      setVisible(false)
    }
  }, [cargando, demoraMs])

  return cargando && visible
}
