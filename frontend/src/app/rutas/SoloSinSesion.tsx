import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { destinoSeguro, useSesion } from '@/shared/sesion'
import { VerificandoSesion } from './PantallasSesion'

/** Login y pantallas similares: con sesión activa se vuelve a donde se iba (o al inicio). */
export function SoloSinSesion() {
  const sesion = useSesion()
  const location = useLocation()

  if (sesion.estado === 'verificando') return <VerificandoSesion />
  if (sesion.estado === 'activa') return <Navigate to={destinoSeguro(location.state)} replace />
  return <Outlet />
}
