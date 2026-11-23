import { useEffect } from 'react'
import { RouterProvider } from 'react-router-dom'
import { restaurarSesion } from '@/shared/sesion'
import { router } from './router'

/** Punto de entrada de la app: aquí se agregan los providers globales. */
export function App() {
  // Al abrir o recargar, recupera la sesión con el refresh guardado (si lo hay).
  useEffect(() => {
    void restaurarSesion()
  }, [])

  return <RouterProvider router={router} />
}
