import { useState } from 'react'
import { extraerError } from '@/shared/api/errores'

export type Mensaje = { tipo: 'exito' | 'error'; texto: string }

/**
 * Activar o desactivar un elemento del catálogo desde una lista: guarda el aviso que se muestra arriba y qué fila
 * está ocupada, y al terminar vuelve a pedir la lista.
 */
export function useCambioEstado(recargar: () => void) {
  const [mensaje, setMensaje] = useState<Mensaje>()
  const [ocupado, setOcupado] = useState<string>()

  const cambiar = async (id: string, nombre: string, activo: boolean, accion: () => Promise<unknown>) => {
    setOcupado(id)
    setMensaje(undefined)
    try {
      await accion()
      setMensaje({ tipo: 'exito', texto: `«${nombre}» quedó ${activo ? 'activo' : 'inactivo'}.` })
      recargar()
    } catch (causa) {
      setMensaje({ tipo: 'error', texto: extraerError(causa).mensaje })
    } finally {
      setOcupado(undefined)
    }
  }
  return { mensaje, setMensaje, ocupado, cambiar }
}
