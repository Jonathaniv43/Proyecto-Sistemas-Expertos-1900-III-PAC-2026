import { useCallback, useEffect } from 'react'
import { usePeticion } from '@/shared/hooks/usePeticion'

/**
 * Una fuente del inicio: se pide al entrar solo si el rol la usa (`activa`). Cada fuente es independiente: si una
 * falla, las demás se muestran igual (design-system.md § Inicio).
 */
export function useFuente<T>(peticion: (signal: AbortSignal) => Promise<T>, activa = true) {
  const { datos, error, cargando, ejecutar } = usePeticion(peticion)
  const cargar = useCallback(() => void ejecutar(), [ejecutar])

  useEffect(() => {
    if (activa) cargar()
  }, [activa, cargar])

  return {
    datos,
    error,
    cargando,
    reintentar: cargar,
    /** Terminó (con datos o con error), o el rol no la usa. */
    lista: !activa || datos !== undefined || error !== undefined,
  }
}
