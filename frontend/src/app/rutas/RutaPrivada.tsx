import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { CampanaNotificaciones } from '@/features/notificaciones/components/CampanaNotificaciones'
import { EncabezadoSesion } from '@/shared/components/EncabezadoSesion'
import { type EstadoLogin, leerUltimaEmpresa, RUTA_CAMBIAR_CONTRASENA, rutaActual, rutaLogin, useSesion } from '@/shared/sesion'
import { SesionSinConexion, VerificandoSesion } from './PantallasSesion'

/**
 * Rutas que requieren sesión. Sin sesión (o cuando vence) lleva al login recordando a dónde
 * volver; el login muestra por qué se pidió ingresar de nuevo. Con contraseña temporal, solo deja
 * usar la pantalla para cambiarla (HU-114).
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
    case 'activa': {
      const enCambio = location.pathname === RUTA_CAMBIAR_CONTRASENA
      if (sesion.usuario.debeCambiarPassword && !enCambio) return <Navigate to={RUTA_CAMBIAR_CONTRASENA} replace />
      if (!sesion.usuario.debeCambiarPassword && enCambio) return <Navigate to="/inicio" replace />
      return (
        <>
          <EncabezadoSesion
            usuario={sesion.usuario}
            // HU-117: solo las sesiones de empresa tienen bandeja; con contraseña temporal aún no se entra a ella.
            acciones={sesion.usuario.tipo === 'empresa' && !sesion.usuario.debeCambiarPassword && <CampanaNotificaciones />}
          />
          <Outlet />
        </>
      )
    }
  }
}
