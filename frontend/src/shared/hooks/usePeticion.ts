import { useCallback, useEffect, useRef, useState } from 'react'
import { extraerError, type ErrorApi } from '@/shared/api/errores'

type Estado<T> = { cargando: boolean; datos: T | undefined; error: ErrorApi | undefined }

/**
 * Ejecuta una llamada a la API y expone su estado: `cargando` (para el spinner del botón o el
 * skeleton), `datos` y `error` ya convertido con `extraerError`.
 *
 * La función recibe un AbortSignal: si la pantalla se desmonta o se vuelve a ejecutar,
 * la petición anterior se cancela y su resultado se descarta.
 *
 * `ejecutar` devuelve los datos, o undefined si falló o se canceló.
 */
export function usePeticion<T, A extends unknown[] = []>(
  peticion: (signal: AbortSignal, ...args: A) => Promise<T>,
) {
  const [estado, setEstado] = useState<Estado<T>>({
    cargando: false,
    datos: undefined,
    error: undefined,
  })
  const controlador = useRef<AbortController | null>(null)
  const peticionActual = useRef(peticion)

  useEffect(() => {
    peticionActual.current = peticion
  })

  useEffect(() => () => controlador.current?.abort(), [])

  const ejecutar = useCallback(async (...args: A): Promise<T | undefined> => {
    controlador.current?.abort()
    const propio = new AbortController()
    controlador.current = propio
    setEstado((previo) => ({ ...previo, cargando: true, error: undefined }))

    try {
      const datos = await peticionActual.current(propio.signal, ...args)
      if (propio.signal.aborted) return undefined
      setEstado({ cargando: false, datos, error: undefined })
      return datos
    } catch (causa) {
      const error = extraerError(causa)
      if (!propio.signal.aborted && !error.cancelada) {
        setEstado((previo) => ({ ...previo, cargando: false, error }))
      }
      return undefined
    }
  }, [])

  return { ...estado, ejecutar }
}
