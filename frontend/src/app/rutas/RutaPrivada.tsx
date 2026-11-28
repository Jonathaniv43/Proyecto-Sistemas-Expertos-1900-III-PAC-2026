import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { type EstadoLogin, leerUltimaEmpresa, rutaActual, rutaLogin, useSesion } from '@/shared/sesion'
import { MarcoSesion } from './MarcoSesion'
import { SesionSinConexion, VerificandoSesion } from './PantallasSesion'

/**
 * Rutas que requieren sesión, dentro del layout de sesión (menú del rol). Sin sesión (o cuando vence) lleva al
 * login recordando a dónde volver; el login muestra por qué se pidió ingresar de nuevo.
 */
export function RutaPrivada() {
  const sesion = useSesion()
  const location = useLocation()

  switch (sesion.estado) {
    case 'verificando':
      return (
        <div className="pagina mx-auto w-full max-w-layout">
          <VerificandoSesion />
        </div>
      )
    case 'sin_conexion':
      return (
        <div className="pagina mx-auto w-full max-w-layout">
          <SesionSinConexion />
        </div>
      )
    case 'sin_sesion': {
      // Se vuelve a la página donde estaba solo si la sesión se cortó sola (venció, se reemplazó o
      // nunca hubo). Tras cerrar sesión o cambiar la contraseña, el próximo ingreso empieza en el inicio:
      // puede entrar otra persona, con otro rol.
      const volver = sesion.motivo !== 'cerrada' && sesion.motivo !== 'contrasena_cambiada'
      const estado: EstadoLogin = { desde: volver ? rutaActual(location) : undefined, motivo: sesion.motivo, rtn: sesion.rtn }
      // Si la empresa de la sesión que terminó es la última resuelta aquí, se va directo a su login.
      const ultima = leerUltimaEmpresa()
      const idEmpresa = ultima && ultima.rtn === sesion.rtn ? ultima.idEmpresa : undefined
      return <Navigate to={rutaLogin(idEmpresa)} replace state={estado} />
    }
    case 'activa':
      return (
        <MarcoSesion usuario={sesion.usuario}>
          <Outlet />
        </MarcoSesion>
      )
  }
}
