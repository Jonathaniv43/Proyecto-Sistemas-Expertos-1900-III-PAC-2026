import type { MotivoFinSesion } from '@/shared/api'
import { Aviso } from '@/shared/components/ui/Aviso'

/** Explica en el login por qué se pidió iniciar sesión de nuevo. "cerrada" no necesita aviso. */
export function AvisoMotivo({ motivo }: { motivo?: MotivoFinSesion }) {
  switch (motivo) {
    case 'expirada':
      return <Aviso tipo="info">Tu sesión terminó. Vuelve a iniciar sesión para continuar.</Aviso>
    case 'reemplazada':
      return (
        <Aviso tipo="info">
          Se inició otra sesión en este navegador, así que esta se cerró. Para trabajar en dos empresas a la vez, usa
          otro navegador o una ventana privada.
        </Aviso>
      )
    case 'contrasena_cambiada':
      return (
        <Aviso tipo="exito" titulo="Tu contraseña quedó cambiada">
          Inicia sesión con tu contraseña nueva. Por seguridad cerramos tus sesiones en todas tus empresas.
        </Aviso>
      )
    default:
      return null
  }
}
