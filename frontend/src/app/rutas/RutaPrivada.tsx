import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { type EstadoLogin, RUTA_LOGIN, rutaActual, useSesion } from '@/shared/sesion'
import { SesionSinConexion, VerificandoSesion } from './PantallasSesion'

/**
 * Rutas que requieren sesión. Sin sesión (o cuando vence) lleva al login recordando a dónde
 * volver; el login muestra por qué se pidió ingresar de nuevo.
 */
export function RutaPrivada() {
  const sesion = useSesion()
  const location = useLocation()

  switch (sesion.estado) {
    case 'verificando':
      return <VerificandoSesion />
    case 'sin_conexion':
      return <SesionSinConexion />
    case 'sin_sesion': {
      const estado: EstadoLogin = { desde: rutaActual(location), motivo: sesion.motivo }
      return <Navigate to={RUTA_LOGIN} replace state={estado} />
    }
    case 'activa':
      return <Outlet />
  }
}
