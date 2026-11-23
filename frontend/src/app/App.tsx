import { RouterProvider } from 'react-router-dom'
import { router } from './router'

/** Punto de entrada de la app: aquí se agregan los providers globales (sesión, consultas…). */
export function App() {
  return <RouterProvider router={router} />
}
